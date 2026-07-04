using Asp.Versioning;
using DevHabit.Api.Database;
using DevHabit.Api.DTOs.Habits;
using DevHabit.Api.Extensions;
using DevHabit.Api.Jobs;
using DevHabit.Api.Middleware;
using DevHabit.Api.Services;
using DevHabit.Api.Services.Sorting;
using DevHabit.Api.Settings;
using FluentValidation;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.Extensions.Http.Resilience;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json.Serialization;
using Npgsql;
using OpenTelemetry;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Polly;
using Quartz;
using Refit;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.RateLimiting;

namespace DevHabit.Api;

public static class DependencyInjection
{
    public static WebApplicationBuilder AddApiServices(this WebApplicationBuilder builder)
    {
        builder.Services.AddControllers(opt =>
        {
            opt.ReturnHttpNotAcceptable = true;//当http请求要求返回一个不受支持的格式类型时，会返回406
        })
            .AddNewtonsoftJson(opt => opt.SerializerSettings.ContractResolver = new CamelCasePropertyNamesContractResolver());//替换默认的json序列化器
        //.AddXmlSerializerFormatters();支持返回xml格式

        builder.Services.Configure<MvcOptions>(opt =>
        {
            var formatter = opt.OutputFormatters.OfType<NewtonsoftJsonOutputFormatter>().First();
            formatter.SupportedMediaTypes.Add(CustomMediaTypeNames.Application.JsonV1);
            formatter.SupportedMediaTypes.Add(CustomMediaTypeNames.Application.JsonV2);
            formatter.SupportedMediaTypes.Add(CustomMediaTypeNames.Application.HateoasJson);
            formatter.SupportedMediaTypes.Add(CustomMediaTypeNames.Application.HateoasJsonV1);
            formatter.SupportedMediaTypes.Add(CustomMediaTypeNames.Application.HateoasJsonV2);
        });

        builder.Services.AddApiVersioning(opt =>
        {
            opt.DefaultApiVersion = new ApiVersion(1.0);
            opt.AssumeDefaultVersionWhenUnspecified = true;
            opt.ReportApiVersions = true;//在响应中附带版本号
            //opt.ApiVersionSelector = new CurrentImplementationApiVersionSelector(opt);
            opt.ApiVersionSelector = new DefaultApiVersionSelector(opt);//当客户端没有指定版本时，使用默认版本
            opt.ApiVersionReader = ApiVersionReader.Combine(
                new MediaTypeApiVersionReader(),
                new MediaTypeApiVersionReaderBuilder().Template("application/vnd.dev-habit.hateoas.{version}+json").Build());
        }).AddMvc();

        builder.Services.AddOpenApi();

        builder.Services.AddResponseCaching();

        return builder;
    }

    public static WebApplicationBuilder AddErrorHandling(this WebApplicationBuilder builder)
    {
        //定制化返回给客户端的问题、错误信息
        builder.Services.AddProblemDetails(opt =>
        {
            opt.CustomizeProblemDetails = context =>
            {
                context.ProblemDetails.Extensions.TryAdd("requestId", context.HttpContext.TraceIdentifier);
            };
        });

        //下面2个错误处理程序按注册的顺序运行
        builder.Services.AddExceptionHandler<ValidationExceptionHandler>();
        builder.Services.AddExceptionHandler<GlobalExceptionHandler>();//添加全局异常处理器实现
        return builder;
    }

    public static WebApplicationBuilder AddDatabase(this WebApplicationBuilder builder)
    {
        var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
        builder.Services.AddDbContext<ApplicationDbContext>(opt =>
        {
            opt.UseNpgsql(connectionString, npgsqlOpt =>
                    npgsqlOpt.MigrationsHistoryTable(HistoryRepository.DefaultTableName, Schemas.Application))
                .UseSnakeCaseNamingConvention();
        });

        builder.Services.AddDbContext<AppIdentityDbContext>(opt =>
        {
            opt.UseNpgsql(connectionString, npgsqlOpt =>
                    npgsqlOpt.MigrationsHistoryTable(HistoryRepository.DefaultTableName, Schemas.Identity))
                .UseSnakeCaseNamingConvention();
        });

        return builder;
    }

    public static WebApplicationBuilder AddObservability(this WebApplicationBuilder builder)
    {
        builder.Services.AddOpenTelemetry()
            .ConfigureResource(b => b.AddService(builder.Environment.ApplicationName))
            .WithTracing(b => b.AddHttpClientInstrumentation()
                .AddAspNetCoreInstrumentation()
                .AddNpgsql())
            .WithMetrics(b => b.AddHttpClientInstrumentation()
                .AddAspNetCoreInstrumentation()
                .AddRuntimeInstrumentation())
            .UseOtlpExporter();

        builder.Logging.AddOpenTelemetry(opt =>
        {
            opt.IncludeScopes = true;
            opt.IncludeFormattedMessage = true;
        });

        return builder;
    }

    public static WebApplicationBuilder AddApplicationServices(this WebApplicationBuilder builder)
    {
        builder.Services.AddValidatorsFromAssemblyContaining<Program>();
        builder.Services.AddTransient<SortMappingProvider>();
        builder.Services.AddSingleton<ISortMappingDefinition, SortMappingDefinition>(_ => HabitMappings.SortMapping);
        builder.Services.AddTransient<DataShapingService>();
        builder.Services.AddHttpContextAccessor();
        builder.Services.AddTransient<LinkService>();

        builder.Services.AddTransient<TokenProvider>();

        builder.Services.AddScoped<UserContext>();//只能在当前请求的作用域内访问

        builder.Services.AddMemoryCache();

        builder.Services.AddScoped<GitHubAccessTokenService>();
        builder.Services.AddTransient<GitHubService>();
        builder.Services.AddTransient<RefitGitHubService>();

        builder.Services.AddHttpClient().ConfigureHttpClientDefaults(e => e.AddStandardResilienceHandler());//添加标准弹性处理器，覆盖全局
        //官方推荐使用标准弹性处理器！！！

        builder.Services.AddHttpClient("github")
            .ConfigureHttpClient(client =>
            {
                client.BaseAddress = new Uri(builder.Configuration.GetSection("GitHub:BaseUrl").Get<string>()!);
                //client.DefaultRequestHeaders.UserAgent.Add(new ProductInfoHeaderValue("DevHabit", "1.0"));
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/vnd.github+json"));
            });

        //使用RefitClient以实现与上面的相同的功能
#pragma warning disable EXTEXP0001 // 类型仅用于评估，在将来的更新中可能会被更改或删除。RemoveAllResilienceHandlers()

        builder.Services.AddRefitClient<IGitHubApi>(new RefitSettings
        {
            ContentSerializer = new NewtonsoftJsonContentSerializer()
        }).ConfigureHttpClient(client =>
            client.BaseAddress = new Uri(builder.Configuration.GetSection("GitHub:BaseUrl").Get<string>()!))


        //因上面的标准弹性处理器是全局的，后续的自定义弹性管道不能覆盖上面全局的，需用下一行方法移除已存在的弹性处理器
        .RemoveAllResilienceHandlers()//此方法是实验性质的，需要抑制警告！谨慎使用


        //为httpclient定义一个弹性管道
        //如果遇到异常，弹性策略会启动
        .AddResilienceHandler("custom", pipe =>
        {
            pipe.AddTimeout(TimeSpan.FromSeconds(5));//全局超时策略。所有重试必须在最多5秒内完成
            pipe.AddRetry(new HttpRetryStrategyOptions
            {
                MaxRetryAttempts = 5,
                BackoffType = DelayBackoffType.Exponential,
                UseJitter = true,
                Delay = TimeSpan.FromMilliseconds(500)
            });
            //引入断路器，确保下游服务持续可用。如果下游服务多次请求失败，断路器会启动，停止向外部服务发请求
            pipe.AddCircuitBreaker(new HttpCircuitBreakerStrategyOptions
            {
                SamplingDuration = TimeSpan.FromSeconds(5),
                FailureRatio = 0.9,
                MinimumThroughput = 5,
                BreakDuration = TimeSpan.FromSeconds(5)
            });
            pipe.AddTimeout(TimeSpan.FromSeconds(1));//后续api请求及重试必须在最多1秒内完成
        });
#pragma warning restore EXTEXP0001 // 类型仅用于评估，在将来的更新中可能会被更改或删除。RemoveAllResilienceHandlers()

        builder.Services.Configure<EncryptionOptions>(builder.Configuration.GetSection("Encryption"));
        builder.Services.AddTransient<EncryptionService>();

        builder.Services.AddSingleton<InMemoryETagStore>();

        return builder;
    }

    public static WebApplicationBuilder AddAuthenticationServices(this WebApplicationBuilder builder)
    {
        builder.Services
            .AddIdentity<IdentityUser, IdentityRole>()//指定用户类型和角色类型
            .AddEntityFrameworkStores<AppIdentityDbContext>();

        builder.Services.Configure<JwtAuthOptions>(builder.Configuration.GetSection("Jwt"));
        var jwtAuthOptions = builder.Configuration.GetSection("Jwt").Get<JwtAuthOptions>() ?? throw new Exception("jwt配置项读取为空！");

        builder.Services
            .AddAuthentication(opt =>
            {
                opt.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                //如果不设置默认质询方案为jwt方案，应用检测到用户的请求未认证时，会跳转到默认登陆页面
                opt.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(opt =>
            {
                opt.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidIssuer = jwtAuthOptions.Issuer,
                    ValidAudience = jwtAuthOptions.Audience,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtAuthOptions.Key))
                };
            });

        builder.Services.AddAuthorization();

        return builder;
    }

    public static WebApplicationBuilder AddBackgroundJobs(this WebApplicationBuilder builder)
    {
        builder.Services.AddQuartz(e =>
        {
            e.AddJob<GitHubAutomationSchedulerJob>(opt => opt.WithIdentity("github-automation-scheduler"));

            e.AddTrigger(opt => opt.ForJob("github-automation-scheduler")
            .WithIdentity("github-automation-scheduler-trigger")
            .WithSimpleSchedule(s =>
            {
                var settings = builder.Configuration.GetSection(GitHubAutomationOptions.SectionName).Get<GitHubAutomationOptions>()!;
                s.WithIntervalInMinutes(settings.ScanIntervalMinutes)
                .RepeatForever();
            }));


            //清理导入任务
            e.AddJob<CleanupEntryImportJobsJob>(opt => opt.WithIdentity("cleanup-entry-imports"));
            e.AddTrigger(opt => opt
                .ForJob("cleanup-entry-imports")
                .WithIdentity("cleanup-entry-imports-trigger")
                .WithCronSchedule("0 0 3 * * ?", x => x.InTimeZone(TimeZoneInfo.Utc)));
        });

        builder.Services.AddQuartzHostedService(e => e.WaitForJobsToComplete = true);

        return builder;
    }


    public static WebApplicationBuilder AddCorsPolicy(this WebApplicationBuilder builder)
    {
        var corsOptions = builder.Configuration.GetSection(CorsOptions.SectionName).Get<CorsOptions>();

        builder.Services.AddCors(opt =>
        {
            opt.AddPolicy(CorsOptions.PolicyName, p =>
            {
                p.WithOrigins(corsOptions!.AllowedOrigins)
                .AllowAnyMethod()
                .AllowAnyHeader();
            });
        });
        return builder;
    }

    public static WebApplicationBuilder AddRateLimiting(this WebApplicationBuilder builder)
    {
        builder.Services.AddRateLimiter(opt =>
        {
            opt.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

            //请求受限后需要自定义的操作
            opt.OnRejected = async (context, token) =>
            {
                if (context.Lease.TryGetMetadata(MetadataName.RetryAfter, out TimeSpan retryAfter))
                {
                    context.HttpContext.Response.Headers.RetryAfter = $"{retryAfter.TotalSeconds}";

                    var problemDetailsFactory = context.HttpContext.RequestServices.GetRequiredService<ProblemDetailsFactory>();
                    var problemDetails = problemDetailsFactory.CreateProblemDetails(context.HttpContext,
                        StatusCodes.Status429TooManyRequests,
                        "请求过多！",
                        detail: $"请求过多！请在{retryAfter.TotalSeconds}秒后再试。");
                    await context.HttpContext.Response.WriteAsJsonAsync(problemDetails, token);
                }
            };

            opt.AddPolicy("default", context =>
            {
                var identityId = context.User.GetIdentityId();
                if (!string.IsNullOrEmpty(identityId))
                {
                    return RateLimitPartition.GetTokenBucketLimiter(identityId, _ =>
                    new TokenBucketRateLimiterOptions
                    {
                        TokenLimit = 100,
                        QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                        QueueLimit = 5,
                        ReplenishmentPeriod = TimeSpan.FromMinutes(1),
                        TokensPerPeriod = 25
                    });//对于认证用户，使用令牌桶限制器
                }

                return RateLimitPartition.GetFixedWindowLimiter("anonymous", _ =>
                    new FixedWindowRateLimiterOptions
                    {
                        PermitLimit = 5,
                        Window = TimeSpan.FromMinutes(1)
                    });//配置为每分钟只能发5个请求
            });
        });
        return builder;
    }
}