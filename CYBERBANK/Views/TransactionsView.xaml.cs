using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using CyberBank.Services;

namespace CyberBank.Views
{
    public partial class TransactionsView : UserControl
    {
        private string _selectedCategory = "All";

        public TransactionsView()
        {
            InitializeComponent();
            Loaded += (s, e) => RefreshData();
            BankService.Instance.OnDataChanged += RefreshData;
        }

        private void RefreshData()
        {
            var user = BankService.Instance.User;
            var query = TxtSearch.Text.Trim().ToLower();

            var list = user.Transactions.AsEnumerable();

            // Filter by category
            if (_selectedCategory != "All")
            {
                if (_selectedCategory == "Maaş")
                {
                    list = list.Where(t => t.Category == "Maaş" || t.IsIncome);
                }
                else
                {
                    list = list.Where(t => t.Category.Equals(_selectedCategory, StringComparison.OrdinalIgnoreCase));
                }
            }

            // Filter by keyword
            if (!string.IsNullOrWhiteSpace(query))
            {
                list = list.Where(t =>
                    t.Title.ToLower().Contains(query) ||
                    t.Description.ToLower().Contains(query) ||
                    t.ReceiptNumber.ToLower().Contains(query) ||
                    t.Category.ToLower().Contains(query));
            }

            var resultList = list.ToList();
            TransactionsList.ItemsSource = null;
            TransactionsList.ItemsSource = resultList;

            // Stats
            decimal totalIncome = user.Transactions.Where(t => t.IsIncome).Sum(t => t.Amount);
            decimal totalExpense = user.Transactions.Where(t => !t.IsIncome).Sum(t => t.Amount);

            TxtTotalIncome.Text = $"+ ₺{totalIncome:N2}";
            TxtTotalExpense.Text = $"- ₺{totalExpense:N2}";
        }

        private void TxtSearch_TextChanged(object sender, TextChangedEventArgs e)
        {
            RefreshData();
        }

        private void CategoryFilter_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is string tag)
            {
                _selectedCategory = tag;
                RefreshData();
            }
        }
    }
}
