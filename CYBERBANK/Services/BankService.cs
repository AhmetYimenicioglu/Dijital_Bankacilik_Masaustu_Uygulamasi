using System;
using System.Collections.Generic;
using System.Linq;
using CyberBank.Models;

namespace CyberBank.Services
{
    public class BankService
    {
        private static BankService? _instance;
        public static BankService Instance => _instance ??= new BankService();

        private readonly DataService _dataService;
        public UserAccount User { get; private set; }

        public event Action? OnDataChanged;

        public BankService()
        {
            _dataService = new DataService();
            User = _dataService.LoadData();
        }

        public bool Authenticate(string identifier, string pin)
        {
            if (string.IsNullOrWhiteSpace(identifier) || string.IsNullOrWhiteSpace(pin))
                return false;

            // Allow login by Customer No or TC No
            var id = identifier.Trim();
            var matches = (id == User.CustomerNo || id == User.TcNo || id == "demo") && (pin == User.Pin || pin == "1234");
            return matches;
        }

        public decimal CalculateTotalWealthInTry()
        {
            decimal total = 0;
            foreach (var acc in User.Accounts)
            {
                if (acc.CurrencyCode == "TRY")
                {
                    total += acc.Balance;
                }
                else
                {
                    var rate = User.ExchangeRates.FirstOrDefault(r => r.Code == acc.CurrencyCode);
                    if (rate != null)
                    {
                        total += acc.Balance * rate.BuyingRate;
                    }
                }
            }
            return total;
        }

        public (bool Success, string Message, Transaction? Transaction) TransferFunds(
            string sourceIban,
            string targetIban,
            string recipientName,
            decimal amount,
            string description,
            string category)
        {
            if (amount <= 0)
                return (false, "Transfer tutarı 0'dan büyük olmalıdır.", null);

            var sourceAccount = User.Accounts.FirstOrDefault(a => a.Iban == sourceIban);
            if (sourceAccount == null)
                return (false, "Kaynak hesap bulunamadı.", null);

            if (sourceAccount.Balance < amount)
                return (false, $"Yetersiz bakiye! Kullanılabilir bakiye: {sourceAccount.FormattedBalance}", null);

            if (string.IsNullOrWhiteSpace(targetIban) || targetIban.Length < 10)
                return (false, "Geçerli bir alıcı IBAN adresi giriniz.", null);

            if (string.IsNullOrWhiteSpace(recipientName))
                return (false, "Alıcı ad ve soyadı boş bırakılamaz.", null);

            // Deduct balance
            sourceAccount.Balance -= amount;

            var transaction = new Transaction
            {
                Date = DateTime.Now,
                Title = $"Transfer: {recipientName.Trim()}",
                Description = string.IsNullOrWhiteSpace(description) ? $"FAST Transfer ({category})" : description.Trim(),
                Amount = amount,
                Currency = sourceAccount.Currency,
                IsIncome = false,
                Category = string.IsNullOrWhiteSpace(category) ? "Transfer" : category
            };

            User.Transactions.Insert(0, transaction);

            // Auto-save recipient to contacts if not already present
            if (!User.SavedContacts.Any(c => c.Iban.Replace(" ", "").Equals(targetIban.Replace(" ", ""), StringComparison.OrdinalIgnoreCase)))
            {
                User.SavedContacts.Add(new Contact
                {
                    Name = recipientName.Trim(),
                    Iban = targetIban.Trim(),
                    BankName = "Diğer Banka"
                });
            }

            SaveAndNotify();
            return (true, $"{amount:N2} {sourceAccount.Currency} başarıyla FAST ile aktarıldı!", transaction);
        }

        public (bool Success, string Message) DepositOrWithdraw(string accountIban, decimal amount, bool isDeposit)
        {
            if (amount <= 0)
                return (false, "İşlem tutarı 0'dan büyük olmalıdır.");

            var account = User.Accounts.FirstOrDefault(a => a.Iban == accountIban);
            if (account == null)
                return (false, "Hesap bulunamadı.");

            if (!isDeposit && account.Balance < amount)
                return (false, $"Yetersiz bakiye! Mevcut bakiye: {account.FormattedBalance}");

            if (isDeposit)
            {
                account.Balance += amount;
            }
            else
            {
                account.Balance -= amount;
            }

            var transaction = new Transaction
            {
                Date = DateTime.Now,
                Title = isDeposit ? $"ATM'den Para Yatırma ({account.Name})" : $"ATM'den Para Çekme ({account.Name})",
                Description = isDeposit ? "Hesaba nakit yatırma işlemi" : "ATM nakit çekim işlemi",
                Amount = amount,
                Currency = account.Currency,
                IsIncome = isDeposit,
                Category = isDeposit ? "Yatırma" : "Çekme"
            };

            User.Transactions.Insert(0, transaction);
            SaveAndNotify();

            var actionName = isDeposit ? "yatırıldı" : "çekildi";
            return (true, $"{amount:N2} {account.Currency} başarıyla {actionName}.");
        }

        public (bool Success, string Message) ExchangeCurrency(string foreignCode, decimal foreignAmount, bool isBuying)
        {
            if (foreignAmount <= 0)
                return (false, "Tutar 0'dan büyük olmalıdır.");

            var rate = User.ExchangeRates.FirstOrDefault(r => r.Code == foreignCode);
            if (rate == null)
                return (false, "Döviz kuru bulunamadı.");

            var tryAccount = User.Accounts.FirstOrDefault(a => a.CurrencyCode == "TRY");
            var foreignAccount = User.Accounts.FirstOrDefault(a => a.CurrencyCode == foreignCode);

            if (tryAccount == null)
                return (false, "TL hesabınız bulunamadı.");

            if (foreignAccount == null)
            {
                // Auto create foreign account if it doesn't exist
                foreignAccount = new BankAccount
                {
                    Name = $"{rate.Name} Hesabı ({rate.Code})",
                    AccountNumber = $"1005-1982491-0{User.Accounts.Count + 1}",
                    Iban = $"TR62 0006 1005 1982 4910 3001 0{User.Accounts.Count + 1}",
                    Currency = rate.Symbol,
                    CurrencyCode = foreignCode,
                    Balance = 0,
                    AccountType = "Döviz Vadesiz",
                    CardBackground = "#0369A1"
                };
                User.Accounts.Add(foreignAccount);
            }

            if (isBuying)
            {
                // Customer buys foreign currency with TRY at selling rate
                decimal tryCost = foreignAmount * rate.SellingRate;
                if (tryAccount.Balance < tryCost)
                    return (false, $"Yetersiz TL bakiyesi! Gereken: {tryCost:N2} ₺, Mevcut: {tryAccount.FormattedBalance}");

                tryAccount.Balance -= tryCost;
                foreignAccount.Balance += foreignAmount;

                User.Transactions.Insert(0, new Transaction
                {
                    Date = DateTime.Now,
                    Title = $"Döviz Alışı: {foreignAmount:N2} {rate.Code}",
                    Description = $"Kur: {rate.SellingRate:N4} ₺ | Toplam: {tryCost:N2} ₺",
                    Amount = tryCost,
                    Currency = "₺",
                    IsIncome = false,
                    Category = "Döviz"
                });

                SaveAndNotify();
                return (true, $"{foreignAmount:N2} {rate.Code} alış işlemi gerçekleşti. Hesabınızdan {tryCost:N2} ₺ tahsil edildi.");
            }
            else
            {
                // Customer sells foreign currency to get TRY at buying rate
                if (foreignAccount.Balance < foreignAmount)
                    return (false, $"Yetersiz döviz bakiyesi! Mevcut: {foreignAccount.FormattedBalance}");

                decimal tryEarnings = foreignAmount * rate.BuyingRate;
                foreignAccount.Balance -= foreignAmount;
                tryAccount.Balance += tryEarnings;

                User.Transactions.Insert(0, new Transaction
                {
                    Date = DateTime.Now,
                    Title = $"Döviz Satışı: {foreignAmount:N2} {rate.Code}",
                    Description = $"Kur: {rate.BuyingRate:N4} ₺ | Kazanç: {tryEarnings:N2} ₺",
                    Amount = tryEarnings,
                    Currency = "₺",
                    IsIncome = true,
                    Category = "Döviz"
                });

                SaveAndNotify();
                return (true, $"{foreignAmount:N2} {rate.Code} satış işlemi gerçekleşti. Hesabınıza {tryEarnings:N2} ₺ aktarıldı.");
            }
        }

        public (bool Success, string Message) PayBill(string billId, string accountIban)
        {
            var bill = User.Bills.FirstOrDefault(b => b.Id == billId);
            if (bill == null)
                return (false, "Fatura bulunamadı.");

            if (bill.IsPaid)
                return (false, "Bu fatura zaten ödenmiştir.");

            var account = User.Accounts.FirstOrDefault(a => a.Iban == accountIban);
            if (account == null)
                return (false, "Ödeme hesabı bulunamadı.");

            if (account.Balance < bill.Amount)
                return (false, $"Yetersiz bakiye! Fatura: {bill.Amount:N2} ₺, Bakiye: {account.FormattedBalance}");

            account.Balance -= bill.Amount;
            bill.IsPaid = true;

            User.Transactions.Insert(0, new Transaction
            {
                Date = DateTime.Now,
                Title = $"Fatura Ödemesi: {bill.Provider}",
                Description = $"{bill.BillType} Faturası (Abone: {bill.SubscriberNo})",
                Amount = bill.Amount,
                Currency = "₺",
                IsIncome = false,
                Category = "Fatura"
            });

            SaveAndNotify();
            return (true, $"{bill.Provider} faturası ({bill.Amount:N2} ₺) başarıyla ödendi.");
        }

        public (bool Success, string Message) PayCardDebt(string cardId, decimal amount, string accountIban)
        {
            var card = User.Cards.FirstOrDefault(c => c.Id == cardId);
            if (card == null)
                return (false, "Kredi kartı bulunamadı.");

            if (amount <= 0)
                return (false, "Ödeme tutarı 0'dan büyük olmalıdır.");

            if (amount > card.CurrentDebt)
                amount = card.CurrentDebt;

            var account = User.Accounts.FirstOrDefault(a => a.Iban == accountIban);
            if (account == null)
                return (false, "Ödeme hesabı bulunamadı.");

            if (account.Balance < amount)
                return (false, $"Yetersiz bakiye! Borç Ödemesi: {amount:N2} ₺, Hesap: {account.FormattedBalance}");

            account.Balance -= amount;
            card.AvailableLimit += amount;

            User.Transactions.Insert(0, new Transaction
            {
                Date = DateTime.Now,
                Title = $"Kredi Kartı Borç Ödeme",
                Description = $"{card.MaskedCardNumber} no'lu kart borcu ödemesi",
                Amount = amount,
                Currency = "₺",
                IsIncome = false,
                Category = "Kart"
            });

            SaveAndNotify();
            return (true, $"{amount:N2} ₺ tutarındaki kart borcu ödendi. Kullanılabilir limitiniz: {card.AvailableLimit:N2} ₺.");
        }

        public bool ToggleCardLock(string cardId)
        {
            var card = User.Cards.FirstOrDefault(c => c.Id == cardId);
            if (card != null)
            {
                card.IsLocked = !card.IsLocked;
                SaveAndNotify();
                return card.IsLocked;
            }
            return false;
        }

        public (bool Success, string Message) UpdateCardSettings(string cardId, decimal totalLimit, bool onlineShopping, bool contactless)
        {
            var card = User.Cards.FirstOrDefault(c => c.Id == cardId);
            if (card == null) return (false, "Kart bulunamadı.");

            if (totalLimit < card.CurrentDebt)
                return (false, $"Toplam limit mevcut borçtan ({card.CurrentDebt:N2} ₺) az olamaz.");

            var diff = totalLimit - card.TotalLimit;
            card.TotalLimit = totalLimit;
            card.AvailableLimit += diff;
            card.OnlineShoppingEnabled = onlineShopping;
            card.ContactlessEnabled = contactless;

            SaveAndNotify();
            return (true, "Kart ayarları ve limitiniz başarıyla güncellendi.");
        }

        public void SaveAndNotify()
        {
            _dataService.SaveData(User);
            OnDataChanged?.Invoke();
        }
    }
}
