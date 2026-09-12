using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using CyberBank.Services;
using CyberBank.Views;

namespace CyberBank
{
    public partial class MainWindow : Window
    {
        private readonly DashboardView _dashboardView;
        private readonly TransferView _transferView;
        private readonly CardsView _cardsView;
        private readonly TransactionsView _transactionsView;
        private readonly ExchangeView _exchangeView;
        private readonly BillsView _billsView;

        private readonly DispatcherTimer _clockTimer;

        public MainWindow()
        {
            InitializeComponent();

            _dashboardView = new DashboardView();
            _transferView = new TransferView();
            _cardsView = new CardsView();
            _transactionsView = new TransactionsView();
            _exchangeView = new ExchangeView();
            _billsView = new BillsView();

            // Hook up intra-app navigation from Dashboard quick action buttons
            _dashboardView.NavigateRequested += SwitchView;

            // Hook up login event
            LoginControl.LoginSuccessful += OnLoginSuccess;

            // Live Clock
            _clockTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(1)
            };
            _clockTimer.Tick += (s, e) =>
            {
                TxtCurrentDateTime.Text = DateTime.Now.ToString("dd MMMM yyyy, HH:mm:ss");
            };
            _clockTimer.Start();
        }

        private void OnLoginSuccess()
        {
            var user = BankService.Instance.User;
            TxtUserFullName.Text = user.FullName;
            TxtUserCustomerNo.Text = $"No: {user.CustomerNo}";

            LoginContainer.Visibility = Visibility.Collapsed;
            MainShell.Visibility = Visibility.Visible;

            SwitchView("Dashboard");
        }

        private void BtnLogout_Click(object sender, RoutedEventArgs e)
        {
            var result = MessageBox.Show("Oturumunuzu kapatmak istediğinize emin misiniz?", "Güvenli Çıkış", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (result == MessageBoxResult.Yes)
            {
                MainShell.Visibility = Visibility.Collapsed;
                LoginContainer.Visibility = Visibility.Visible;
            }
        }

        private void Nav_Click(object sender, RoutedEventArgs e)
        {
            if (sender is RadioButton rb && rb.Tag is string destination)
            {
                SwitchView(destination);
            }
        }

        public void SwitchView(string viewName)
        {
            switch (viewName)
            {
                case "Dashboard":
                    ViewHost.Content = _dashboardView;
                    TxtHeaderTitle.Text = "Finansal Gösterge Paneli";
                    NavDashboard.IsChecked = true;
                    _dashboardView.RefreshData();
                    break;

                case "Transfer":
                    ViewHost.Content = _transferView;
                    TxtHeaderTitle.Text = "Para Transferi (FAST & Havale)";
                    NavTransfer.IsChecked = true;
                    break;

                case "Cards":
                    ViewHost.Content = _cardsView;
                    TxtHeaderTitle.Text = "Kart Yönetimi & Güvenlik Ayarları";
                    NavCards.IsChecked = true;
                    break;

                case "Transactions":
                    ViewHost.Content = _transactionsView;
                    TxtHeaderTitle.Text = "Detaylı Hesap Hareketleri";
                    NavTransactions.IsChecked = true;
                    break;

                case "Exchange":
                    ViewHost.Content = _exchangeView;
                    TxtHeaderTitle.Text = "Döviz & Kıymetli Maden Portföyü";
                    NavExchange.IsChecked = true;
                    break;

                case "Bills":
                    ViewHost.Content = _billsView;
                    TxtHeaderTitle.Text = "Faturalar & Kurum Ödemeleri";
                    NavBills.IsChecked = true;
                    break;
            }
        }
    }
}