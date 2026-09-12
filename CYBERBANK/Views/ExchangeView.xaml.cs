using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using CyberBank.Models;
using CyberBank.Services;

namespace CyberBank.Views
{
    public partial class ExchangeView : UserControl
    {
        public ExchangeView()
        {
            InitializeComponent();
            Loaded += (s, e) => RefreshData();
            BankService.Instance.OnDataChanged += RefreshData;
        }

        private void RefreshData()
        {
            var user = BankService.Instance.User;

            RatesList.ItemsSource = null;
            RatesList.ItemsSource = user.ExchangeRates;

            var prevSelected = CmbTradingCurrency.SelectedItem as ExchangeRate;
            CmbTradingCurrency.ItemsSource = null;
            CmbTradingCurrency.ItemsSource = user.ExchangeRates;

            if (prevSelected != null && user.ExchangeRates.Any(r => r.Code == prevSelected.Code))
            {
                CmbTradingCurrency.SelectedItem = user.ExchangeRates.First(r => r.Code == prevSelected.Code);
            }
            else if (user.ExchangeRates.Count > 0)
            {
                CmbTradingCurrency.SelectedIndex = 0;
            }

            UpdateCalculation();
        }

        private void TradeMode_Changed(object sender, RoutedEventArgs e)
        {
            if (!IsLoaded) return;
            bool isBuying = RbBuy.IsChecked == true;
            LblAmountTitle.Text = isBuying ? "Alınacak Miktar" : "Satılacak Miktar";
            LblTotalCostTitle.Text = isBuying ? "Ödenecek Toplam Tutar" : "Hesabınıza Geçecek Tutar";
            BtnExecuteTrade.Content = isBuying ? "🟢 Döviz Alışını Tamamla" : "🔴 Döviz Satışını Tamamla";

            UpdateCalculation();
        }

        private void CmbTradingCurrency_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            UpdateCalculation();
        }

        private void TxtTradeAmount_TextChanged(object sender, TextChangedEventArgs e)
        {
            UpdateCalculation();
        }

        private void UpdateCalculation()
        {
            if (CmbTradingCurrency == null || TxtTradeAmount == null || TxtTotalCost == null) return;

            var selectedRate = CmbTradingCurrency.SelectedItem as ExchangeRate;
            if (selectedRate == null) return;

            bool isBuying = RbBuy.IsChecked == true;
            decimal rateValue = isBuying ? selectedRate.SellingRate : selectedRate.BuyingRate;

            TxtEffectiveRate.Text = $"1 {selectedRate.Code} = {rateValue:N2} ₺";

            var user = BankService.Instance.User;
            var tlAcc = user.Accounts.FirstOrDefault(a => a.CurrencyCode == "TRY");
            var forAcc = user.Accounts.FirstOrDefault(a => a.CurrencyCode == selectedRate.Code);

            TxtAvailableTl.Text = tlAcc != null ? tlAcc.FormattedBalance : "₺ 0.00";
            TxtAvailableForeign.Text = forAcc != null ? forAcc.FormattedBalance : $"0.00 {selectedRate.Symbol}";

            if (decimal.TryParse(TxtTradeAmount.Text.Trim(), out decimal amount) && amount > 0)
            {
                decimal total = amount * rateValue;
                TxtTotalCost.Text = $"₺ {total:N2}";
            }
            else
            {
                TxtTotalCost.Text = "₺ 0.00";
            }
        }

        private void BtnExecuteTrade_Click(object sender, RoutedEventArgs e)
        {
            if (CmbTradingCurrency.SelectedItem is not ExchangeRate rate)
            {
                MessageBox.Show("Lütfen bir para birimi seçiniz.", "Uyarı", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!decimal.TryParse(TxtTradeAmount.Text.Trim(), out decimal amount) || amount <= 0)
            {
                MessageBox.Show("Lütfen geçerli bir miktar giriniz.", "Uyarı", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            bool isBuying = RbBuy.IsChecked == true;
            var result = BankService.Instance.ExchangeCurrency(rate.Code, amount, isBuying);

            if (result.Success)
            {
                MessageBox.Show(result.Message, "İşlem Başarılı", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            else
            {
                MessageBox.Show(result.Message, "İşlem Gerçekleştirilemedi", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }
    }
}
