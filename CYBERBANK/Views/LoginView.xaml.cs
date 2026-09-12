using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using CyberBank.Services;

namespace CyberBank.Views
{
    public partial class LoginView : UserControl
    {
        public event Action? LoginSuccessful;

        public LoginView()
        {
            InitializeComponent();
        }

        private void BtnLogin_Click(object sender, RoutedEventArgs e)
        {
            PerformLogin(TxtIdentifier.Text, TxtPin.Password);
        }

        private void BtnDemo_Click(object sender, RoutedEventArgs e)
        {
            TxtIdentifier.Text = "8249103";
            TxtPin.Password = "1234";
            PerformLogin("8249103", "1234");
        }

        private void PerformLogin(string identifier, string pin)
        {
            var success = BankService.Instance.Authenticate(identifier, pin);
            if (success)
            {
                ErrorBorder.Visibility = Visibility.Collapsed;
                LoginSuccessful?.Invoke();
            }
            else
            {
                ErrorBorder.Visibility = Visibility.Visible;
                TxtError.Text = "Geçersiz kimlik veya PIN! (Demo: 8249103 / PIN: 1234)";
            }
        }

        private void TxtPin_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                PerformLogin(TxtIdentifier.Text, TxtPin.Password);
            }
        }

        private void ForgotPin_MouseDown(object sender, MouseButtonEventArgs e)
        {
            MessageBox.Show("Demo ortamında PIN kodunuz: 1234\nMüşteri No: 8249103", "PIN Hatırlatma", MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }
}
