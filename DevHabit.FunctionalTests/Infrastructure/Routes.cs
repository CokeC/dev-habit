namespace DevHabit.FunctionalTests.Infrastructure;

public static class Routes
{
    public static class Auth
    {
        public const string Register = "auth/register";
        public const string Login = "auth/login";
    }

    public static class Habits
    {
        public const string Create = "habits";
        public static string Patch(string id)
        {
            return $"{Create}/{id}";
        }
        public static string GetById(string id)
        {
            return $"{Create}/{id}";
        }
    }

    public static class GitHub
    {
        public const string StoreAccessToken = "github/personal-access-token";
        public const string GetProfile = "github/profile";
        public const string GetEvents = "github/events";
    }

    public static class Entries
    {
        public const string Create = "entries";
        public const string GetAll = "entries";
        public const string Stats = "entries/stats";
    }
}