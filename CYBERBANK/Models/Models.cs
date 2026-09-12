using System;
using System.Collections.Generic;

namespace CyberBank.Models
{
    public class BankAccount
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string Name { get; set; } = string.Empty;
        public string AccountNumber { get; set; } = string.Empty;
        public string Iban { get; set; } = string.Empty;
        public string Currency { get; set; } = "₺";
        public string CurrencyCode { get; set; } = "TRY";
        public decimal Balance { get; set; }
        public string AccountType { get; set; } = "Vadesiz Hesap";
        public string FormattedBalance => $"{Balance:N2} {Currency}";
        public string CardBackground { get; set; } = "#1E293B";
    }

    public class CreditCard
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string CardHolder { get; set; } = "DEMO KULLANICI";
        public string CardNumber { get; set; } = string.Empty;
        public string FormattedCardNumber => FormatNumber(CardNumber);
        public string MaskedCardNumber => MaskNumber(CardNumber);
        public string ExpiryDate { get; set; } = "08/29";
        public string Cvv { get; set; } = "742";
        public decimal TotalLimit { get; set; } = 75000;
        public decimal AvailableLimit { get; set; } = 56820;
        public decimal CurrentDebt => TotalLimit - AvailableLimit;
        public bool IsLocked { get; set; } = false;
        public bool ContactlessEnabled { get; set; } = true;
        public bool OnlineShoppingEnabled { get; set; } = true;

        private static string FormatNumber(string raw)
        {
            if (string.IsNullOrWhiteSpace(raw) || raw.Length < 16) return raw;
            return $"{raw.Substring(0, 4)} {raw.Substring(4, 4)} {raw.Substring(8, 4)} {raw.Substring(12, 4)}";
        }

        private static string MaskNumber(string raw)
        {
            if (string.IsNullOrWhiteSpace(raw) || raw.Length < 16) return raw;
            return $"{raw.Substring(0, 4)} **** **** {raw.Substring(12, 4)}";
        }
    }

    public class Contact
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string Name { get; set; } = string.Empty;
        public string Iban { get; set; } = string.Empty;
        public string BankName { get; set; } = "CyberBank";
        public string Initials
        {
            get
            {
                if (string.IsNullOrWhiteSpace(Name)) return "CB";
                var parts = Name.Trim().Split(' ');
                if (parts.Length == 1) return parts[0].Substring(0, Math.Min(2, parts[0].Length)).ToUpper();
                return $"{parts[0][0]}{parts[^1][0]}".ToUpper();
            }
        }
    }

    public class Transaction
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public DateTime Date { get; set; } = DateTime.Now;
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public string Currency { get; set; } = "₺";
        public bool IsIncome { get; set; }
        public string Category { get; set; } = "Transfer";
        public string ReceiptNumber { get; set; } = $"TRX-{Random.Shared.Next(10000000, 99999999)}";

        public string FormattedAmount => (IsIncome ? "+ " : "- ") + $"{Amount:N2} {Currency}";
        public string AmountColor => IsIncome ? "#10B981" : "#EF4444";
        public string FormattedDate => Date.ToString("dd MMM yyyy, HH:mm");
        public string IconText => Category switch
        {
            "Market" => "🛒",
            "Maaş" => "💼",
            "Kira" => "🏠",
            "Fatura" => "⚡",
            "Yeme-İçme" => "🍔",
            "Döviz" => "💱",
            "Abonelik" => "🎬",
            _ => IsIncome ? "📥" : "📤"
        };
    }

    public class ExchangeRate
    {
        public string Code { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Symbol { get; set; } = string.Empty;
        public decimal BuyingRate { get; set; }
        public decimal SellingRate { get; set; }
        public decimal DailyChangePercent { get; set; }
        public bool IsPositiveChange => DailyChangePercent >= 0;
        public string ChangeSign => DailyChangePercent >= 0 ? $"+%{DailyChangePercent:F2}" : $"-%{Math.Abs(DailyChangePercent):F2}";
        public string ChangeColor => DailyChangePercent >= 0 ? "#10B981" : "#EF4444";
    }

    public class Bill
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string BillType { get; set; } = string.Empty; // Elektrik, Su, Doğalgaz, İnternet
        public string Provider { get; set; } = string.Empty;
        public string SubscriberNo { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public string DueDate { get; set; } = string.Empty;
        public bool IsPaid { get; set; } = false;
        public string IconText => BillType switch
        {
            "Elektrik" => "⚡",
            "Su" => "💧",
            "Doğalgaz" => "🔥",
            "İnternet" => "🌐",
            _ => "🧾"
        };
    }

    public class UserAccount
    {
        public string CustomerNo { get; set; } = "8249103";
        public string TcNo { get; set; } = "12345678901";
        public string FullName { get; set; } = "Demo Müşteri";
        public string Pin { get; set; } = "1234";
        public string Email { get; set; } = "demo@cyberbank.com";
        public string Phone { get; set; } = "+90 (555) 000 00 00";
        public List<BankAccount> Accounts { get; set; } = new();
        public List<CreditCard> Cards { get; set; } = new();
        public List<Contact> SavedContacts { get; set; } = new();
        public List<Transaction> Transactions { get; set; } = new();
        public List<ExchangeRate> ExchangeRates { get; set; } = new();
        public List<Bill> Bills { get; set; } = new();
    }
}
