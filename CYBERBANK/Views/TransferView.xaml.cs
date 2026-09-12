using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using CyberBank.Models;
using CyberBank.Services;

namespace CyberBank.Views
{
    public partial class TransferView : UserControl
    {
        public TransferView()
        {
            InitializeComponent();
            Loaded += (s, e) => RefreshData();
            BankService.Instance.OnDataChanged += RefreshData;
        }

        private void RefreshData()
        {
            var user = BankService.Instance.User;

            // Load TRY Accounts
            var tryAccounts = user.Accounts.Where(a => a.CurrencyCode == "TRY").ToList();
            var prevSelected = CmbSourceAccount.SelectedItem as BankAccount;

            CmbSourceAccount.ItemsSource = null;
            CmbSourceAccount.ItemsSource = tryAccounts;

            if (prevSelected != null && tryAccounts.Any(a => a.Id == prevSelected.Id))
            {
                CmbSourceAccount.SelectedItem = tryAccounts.First(a => a.Id == prevSelected.Id);
            }
            else if (tryAccounts.Count > 0)
            {
                CmbSourceAccount.SelectedIndex = 0;
            }

            // Saved Contacts
            ContactsList.ItemsSource = null;
            ContactsList.ItemsSource = user.SavedContacts;
        }

        private void CmbSourceAccount_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // Optional: update label if needed
        }

        private void ContactButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is Contact contact)
            {
                TxtRecipientName.Text = contact.Name;
                TxtIban.Text = contact.Iban;
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

        private void BtnSendTransfer_Click(object sender, RoutedEventArgs e)
        {
            if (CmbSourceAccount.SelectedItem is not BankAccount sourceAcc)
            {
                MessageBox.Show("Lütfen gönderen hesap seçiniz.", "Uyarı", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var iban = TxtIban.Text.Trim();
            var recipient = TxtRecipientName.Text.Trim();
            var desc = TxtDescription.Text.Trim();
            var category = (CmbCategory.SelectedItem as ComboBoxItem)?.Content?.ToString() ?? "Transfer";

            if (string.IsNullOrWhiteSpace(iban) || iban.Length < 10)
            {
                MessageBox.Show("Lütfen geçerli bir IBAN giriniz.", "Uyarı", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (string.IsNullOrWhiteSpace(recipient))
            {
                MessageBox.Show("Lütfen alıcı adı ve soyadını giriniz.", "Uyarı", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!decimal.TryParse(TxtAmount.Text.Trim(), out decimal amount) || amount <= 0)
            {
                MessageBox.Show("Lütfen geçerli bir transfer tutarı giriniz.", "Uyarı", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var confirmResult = MessageBox.Show(
                $"Alıcı: {recipient}\nIBAN: {iban}\nTutar: {amount:N2} ₺\n\nFAST transferini onaylıyor musunuz?",
                "Transfer Onayı",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (confirmResult != MessageBoxResult.Yes) return;

            var result = BankService.Instance.TransferFunds(
                sourceAcc.Iban,
                iban,
                recipient,
                amount,
                desc,
                category);

            if (result.Success && result.Transaction != null)
            {
                // Show digital receipt
                TxtReceiptNo.Text = result.Transaction.ReceiptNumber;
                TxtReceiptRecipient.Text = recipient;
                TxtReceiptIban.Text = iban.Length > 12 ? $"{iban.Substring(0, 4)}...{iban.Substring(iban.Length - 4)}" : iban;
                TxtReceiptAmount.Text = $"{amount:N2} ₺";
                TxtReceiptDate.Text = result.Transaction.FormattedDate;
                ReceiptCard.Visibility = Visibility.Visible;

                // Clear input
                TxtAmount.Text = string.Empty;

                MessageBox.Show(result.Message, "Transfer Başarılı", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            else
            {
                MessageBox.Show(result.Message, "Transfer Başarısız", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void BtnCopyReceipt_Click(object sender, RoutedEventArgs e)
        {
            Clipboard.SetText(TxtReceiptNo.Text);
            MessageBox.Show("Referans numarası panoya kopyalandı.", "Kopyalandı", MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }
}
