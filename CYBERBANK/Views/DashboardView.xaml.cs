using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using CyberBank.Services;

namespace CyberBank.Views
{
    public partial class DashboardView : UserControl
    {
        public event Action<string>? NavigateRequested;

        public DashboardView()
        {
            InitializeComponent();
            Loaded += (s, e) => RefreshData();
            BankService.Instance.OnDataChanged += RefreshData;
        }

        public void RefreshData()
        {
            var user = BankService.Instance.User;
            var bankService = BankService.Instance;

            // Total Wealth
            var totalTry = bankService.CalculateTotalWealthInTry();
            TxtTotalWealth.Text = $"₺ {totalTry:N2}";

            var tlAcc = user.Accounts.FirstOrDefault(a => a.CurrencyCode == "TRY");
            TxtTlSummary.Text = tlAcc != null ? tlAcc.FormattedBalance : "₺ 0.00";

            var usdAcc = user.Accounts.FirstOrDefault(a => a.CurrencyCode == "USD");
            var eurAcc = user.Accounts.FirstOrDefault(a => a.CurrencyCode == "EUR");
            TxtForeignSummary.Text = $"{(usdAcc != null ? usdAcc.FormattedBalance : "$0")} / {(eurAcc != null ? eurAcc.FormattedBalance : "€0")}";

            var termAcc = user.Accounts.FirstOrDefault(a => a.AccountType.Contains("Vadeli"));
            TxtTermSummary.Text = termAcc != null ? termAcc.FormattedBalance : "₺ 0.00";

            // Accounts List
            AccountsList.ItemsSource = null;
            AccountsList.ItemsSource = user.Accounts;

            // Credit Card
            var card = user.Cards.FirstOrDefault();
            if (card != null)
            {
                TxtCardNumber.Text = card.MaskedCardNumber;
                TxtCardHolder.Text = card.CardHolder;
                TxtCardExpiry.Text = card.ExpiryDate;
                TxtAvailableLimit.Text = $"₺ {card.AvailableLimit:N2}";
                TxtCardDebt.Text = $"₺ {card.CurrentDebt:N2}";

                double ratio = card.TotalLimit > 0 ? (double)(card.AvailableLimit / card.TotalLimit) * 100 : 0;
                PrgCardLimit.Value = Math.Max(0, Math.Min(100, ratio));
            }

            // Recent Transactions (top 4)
            RecentTransactionsList.ItemsSource = null;
            RecentTransactionsList.ItemsSource = user.Transactions.Take(4).ToList();
        }

        private void BtnAtm_Click(object sender, RoutedEventArgs e)
        {
            var atmDialog = new AtmDialog();
            atmDialog.Owner = Window.GetWindow(this);
            atmDialog.ShowDialog();
        }

        private void BtnPayDebt_Click(object sender, RoutedEventArgs e)
        {
            var card = BankService.Instance.User.Cards.FirstOrDefault();
            var tlAccount = BankService.Instance.User.Accounts.FirstOrDefault(a => a.CurrencyCode == "TRY");

            if (card == null || tlAccount == null) return;

            if (card.CurrentDebt <= 0)
            {
                MessageBox.Show("Ödenecek güncel dönem borcunuz bulunmamaktadır.", "Bilgi", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            var result = MessageBox.Show($"Güncel dönem borcunuz olan {card.CurrentDebt:N2} ₺ tutarını {tlAccount.Name} hesabınızdan ödemek istiyor musunuz?", 
                "Borç Ödeme Onayı", MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                var response = BankService.Instance.PayCardDebt(card.Id, card.CurrentDebt, tlAccount.Iban);
                if (response.Success)
                {
                    MessageBox.Show(response.Message, "Başarılı", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    MessageBox.Show(response.Message, "Hata", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }
        }

        private void BtnGoTransfer_Click(object sender, RoutedEventArgs e) => NavigateRequested?.Invoke("Transfer");
        private void BtnGoCards_Click(object sender, RoutedEventArgs e) => NavigateRequested?.Invoke("Cards");
        private void BtnGoExchange_Click(object sender, RoutedEventArgs e) => NavigateRequested?.Invoke("Exchange");
        private void BtnGoBills_Click(object sender, RoutedEventArgs e) => NavigateRequested?.Invoke("Bills");
        private void BtnAllTransactions_Click(object sender, RoutedEventArgs e) => NavigateRequested?.Invoke("Transactions");
    }
}
