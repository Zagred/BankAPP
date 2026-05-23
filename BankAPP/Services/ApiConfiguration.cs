namespace BankAPP.Services
{
    public static class ApiConfiguration
    {
        public static Uri BaseAddress
        {
            get
            {
#if ANDROID
                return new Uri("http://10.0.2.2:5218/");
#else
                return new Uri("https://localhost:7083/");
#endif
            }
        }
    }
}
