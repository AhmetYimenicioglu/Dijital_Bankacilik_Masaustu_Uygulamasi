using System.Windows;
using CyberBank.Services;

namespace CyberBank
{
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            // Pre-initialize BankService to load or seed data
            _ = BankService.Instance;
        }
    }
}
