using BankAPP.Data;

namespace BankAPP
{
    public partial class App : Application
    {
        public App(AppDatabase database)
        {
            InitializeComponent();
            MainPage = new NavigationPage(new LoginPage(database));
        }
    }
}