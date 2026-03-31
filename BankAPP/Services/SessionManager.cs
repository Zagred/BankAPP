namespace BankAPP.Services
{
    public static class SessionManager
    {
        public static int CurrentUserId { get; set; }

        public static string CurrentUsername { get; set; } = string.Empty;

        public static bool IsLoggedIn =>
            CurrentUserId > 0 && !string.IsNullOrWhiteSpace(CurrentUsername);

        public static void Logout()
        {
            CurrentUserId = 0;
            CurrentUsername = string.Empty;
        }
    }
}