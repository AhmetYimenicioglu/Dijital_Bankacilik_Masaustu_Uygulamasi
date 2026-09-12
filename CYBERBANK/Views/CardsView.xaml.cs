using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using CyberBank.Models;
using CyberBank.Services;

namespace CyberBank.Views
{
    public partial class CardsView : UserControl
    {
        private bool _isCardNumberMasked = true;

        public CardsView()
        {
            InitializeComponent();
            Loaded += (s, e) => RefreshData();
            BankService.Instance.OnDataChanged += RefreshData;
        }

        private void RefreshData()
        {
            var card = BankService.Instance.User.Cards.FirstOrDefault();
            if (card == null) return;

            TxtCardHolder.Text = card.CardHolder;
            TxtExpiry.Text = card.ExpiryDate;
            TxtCvv.Text = _isCardNumberMasked ? "***" : card.Cvv;
            TxtCardNumberDisplay.Text = _isCardNumberMasked ? card.MaskedCardNumber : card.FormattedCardNumber;

            TxtTotalLimit.Text = $"₺ {card.TotalLimit:N2}";
            TxtAvailLimit.Text = $"₺ {card.AvailableLimit:N2}";
            TxtDebt.Text = $"₺ {card.CurrentDebt:N2}";

            TxtLimitInput.Text = ((int)card.TotalLimit).ToString();

            // Status Badge
            if (card.IsLocked)
            {
                TxtCardStatus.Text = "KİLİTLİ";
                BadgeCardStatus.Background = (SolidColorBrush)FindResource("BrushDanger");
                ChkLockCard.IsChecked = true;
            }
            else
            {
                TxtCardStatus.Text = "AKTİF";
                BadgeCardStatus.Background = (SolidColorBrush)FindResource("BrushEmerald");
                ChkLockCard.IsChecked = false;
            }

            ChkOnlineShopping.IsChecked = card.OnlineShoppingEnabled;
            ChkContactless.IsChecked = card.ContactlessEnabled;
        }

        private void BtnToggleMask_Click(object sender, RoutedEventArgs e)
        {
            _isCardNumberMasked = !_isCardNumberMasked;
            RefreshData();
        }

        private void ChkLockCard_Click(object sender, RoutedEventArgs e)
        {
            var card = BankService.Instance.User.Cards.FirstOrDefault();
            if (card != null)
            {
                var isLocked = BankService.Instance.ToggleCardLock(card.Id);
                var statusText = isLocked ? "kilitlendi (tüm işlemlere kapatıldı)" : "kilidi açıldı (kullanıma hazır)";
                MessageBox.Show($"Kartınız geçici olarak {statusText}.", "Kart Güvenlik Bildirimi", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void ChkSettings_Click(object sender, RoutedEventArgs e)
        {
            var card = BankService.Instance.User.Cards.FirstOrDefault();
            if (card != null)
            {
                var online = ChkOnlineShopping.IsChecked == true;
                var contactless = ChkContactless.IsChecked == true;
                BankService.Instance.UpdateCardSettings(card.Id, card.TotalLimit, online, contactless);
            }
        }

        private void BtnUpdateLimit_Click(object sender, RoutedEventArgs e)
        {
            var card = BankService.Instance.User.Cards.FirstOrDefault();
            if (card == null) return;

            if (!decimal.TryParse(TxtLimitInput.Text.Trim(), out decimal newLimit) || newLimit <= 0)
            {
                MessageBox.Show("Lütfen geçerli bir limit tutarı giriniz.", "Uyarı", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var res = BankService.Instance.UpdateCardSettings(card.Id, newLimit, card.OnlineShoppingEnabled, card.ContactlessEnabled);
            if (res.Success)
            {
                MessageBox.Show(res.Message, "Limit Güncellendi", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            else
            {
                MessageBox.Show(res.Message, "Hata", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
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

            var result = MessageBox.Show($"Dönem borcunuz olan {card.CurrentDebt:N2} ₺ tutarını {tlAccount.Name} hesabınızdan ödemek istiyor musunuz?",
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
    }
}
