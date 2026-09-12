using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using CyberBank.Models;
using CyberBank.Services;

namespace CyberBank.Views
{
    public partial class BillsView : UserControl
    {
        public BillsView()
        {
            InitializeComponent();
            Loaded += (s, e) => RefreshData();
            BankService.Instance.OnDataChanged += RefreshData;
        }

        private void RefreshData()
        {
            var user = BankService.Instance.User;

            BillsList.ItemsSource = null;
            BillsList.ItemsSource = user.Bills;

            var tryAccounts = user.Accounts.Where(a => a.CurrencyCode == "TRY").ToList();
            CmbPaymentAccount.ItemsSource = null;
            CmbPaymentAccount.ItemsSource = tryAccounts;
            if (tryAccounts.Count > 0)
            {
                CmbPaymentAccount.SelectedIndex = 0;
            }
        }

        private void BtnPayBill_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is Bill bill)
            {
                var tlAccount = BankService.Instance.User.Accounts.FirstOrDefault(a => a.CurrencyCode == "TRY");
                if (tlAccount == null)
                {
                    MessageBox.Show("Ödeme yapılacak TL hesabınız bulunamadı.", "Hata", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                var confirm = MessageBox.Show(
                    $"{bill.Provider} faturasını ({bill.Amount:N2} ₺) {tlAccount.Name} hesabınızdan ödemek istiyor musunuz?",
                    "Fatura Ödeme Onayı",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question);

                if (confirm == MessageBoxResult.Yes)
                {
                    var result = BankService.Instance.PayBill(bill.Id, tlAccount.Iban);
                    if (result.Success)
                    {
                        MessageBox.Show(result.Message, "Ödeme Başarılı", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                    else
                    {
                        MessageBox.Show(result.Message, "Ödeme Başarısız", MessageBoxButton.OK, MessageBoxImage.Warning);
                    }
                }
            }
        }

        private void BtnQueryBill_Click(object sender, RoutedEventArgs e)
        {
            var subNo = TxtSubNo.Text.Trim();
            if (string.IsNullOrWhiteSpace(subNo))
            {
                MessageBox.Show("Lütfen tesisat / abone numaranızı giriniz.", "Uyarı", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var category = (CmbBillCategory.SelectedItem as ComboBoxItem)?.Content?.ToString() ?? "Kurum";
            var randomDebt = (decimal)Random.Shared.Next(180, 850) + (decimal)Random.Shared.Next(0, 99) / 100m;

            var result = MessageBox.Show(
                $"Abone No: {subNo}\nKurum: {category}\n\nGüncel Borç: {randomDebt:N2} ₺\nSon Ödeme: {DateTime.Now.AddDays(15):dd.MM.yyyy}\n\nHemen ödemek ister misiniz?",
                "Fatura Sorgulama Sonucu",
                MessageBoxButton.YesNo,
                MessageBoxImage.Information);

            if (result == MessageBoxResult.Yes)
            {
                if (CmbPaymentAccount.SelectedItem is not BankAccount acc)
                {
                    MessageBox.Show("Lütfen ödeme yapılacak hesabı seçiniz.", "Uyarı", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (acc.Balance < randomDebt)
                {
                    MessageBox.Show($"Yetersiz bakiye! Hesap bakiyeniz: {acc.FormattedBalance}", "Hata", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                // Deduct and create transaction
                acc.Balance -= randomDebt;
                var parts = category.Split('(');
                var providerName = parts.Length > 0 ? parts[0].Trim() : category;

                BankService.Instance.User.Transactions.Insert(0, new Transaction
                {
                    Date = DateTime.Now,
                    Title = $"Fatura Ödemesi: {providerName}",
                    Description = $"Abone No: {subNo} (Online Sorgulama)",
                    Amount = randomDebt,
                    Currency = "₺",
                    IsIncome = false,
                    Category = "Fatura"
                });

                BankService.Instance.SaveAndNotify();
                TxtSubNo.Text = string.Empty;

                MessageBox.Show($"{randomDebt:N2} ₺ tutarındaki faturanız başarıyla ödendi.", "Ödeme Tamamlandı", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }
    }
}
