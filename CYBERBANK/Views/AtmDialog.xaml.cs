using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using CyberBank.Models;
using CyberBank.Services;

namespace CyberBank.Views
{
    public partial class AtmDialog : Window
    {
        public AtmDialog()
        {
            InitializeComponent();
            var accounts = BankService.Instance.User.Accounts.Where(a => a.CurrencyCode == "TRY").ToList();
            CmbAccounts.ItemsSource = accounts;
            if (accounts.Count > 0)
            {
                CmbAccounts.SelectedIndex = 0;
            }
        }

        private void QuickAmount_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn)
            {
                var clean = btn.Content.ToString()?.Replace("₺", "").Replace(".", "").Trim();
                TxtAmount.Text = clean;
            }
        }

        private void BtnConfirm_Click(object sender, RoutedEventArgs e)
        {
            if (CmbAccounts.SelectedItem is not BankAccount selectedAccount)
            {
                MessageBox.Show("Lütfen bir hesap seçiniz.", "Uyarı", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!decimal.TryParse(TxtAmount.Text.Trim(), out decimal amount) || amount <= 0)
            {
                MessageBox.Show("Lütfen geçerli pozitif bir tutar giriniz.", "Uyarı", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            bool isDeposit = RbDeposit.IsChecked == true;
            var result = BankService.Instance.DepositOrWithdraw(selectedAccount.Iban, amount, isDeposit);

            if (result.Success)
            {
                MessageBox.Show(result.Message, "İşlem Başarılı", MessageBoxButton.OK, MessageBoxImage.Information);
                Close();
            }
            else
            {
                MessageBox.Show(result.Message, "İşlem Başarısız", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }
    }
}
