using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using CyberBank.Models;

namespace CyberBank.Services
{
    public class DataService
    {
        private readonly string _filePath;

        public DataService()
        {
            var appData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            var appFolder = Path.Combine(appData, "CyberBank");
            if (!Directory.Exists(appFolder))
            {
                Directory.CreateDirectory(appFolder);
            }
            _filePath = Path.Combine(appFolder, "bank_data.json");
        }

        public UserAccount LoadData()
        {
            try
            {
                if (File.Exists(_filePath))
                {
                    var json = File.ReadAllText(_filePath);
                    var data = JsonSerializer.Deserialize<UserAccount>(json);
                    if (data != null && data.Accounts.Count > 0)
                    {
                        return data;
                    }
                }
            }
            catch
            {
                // Fallback to seed data if file is corrupted
            }

            var initialData = CreateSeedData();
            SaveData(initialData);
            return initialData;
        }

        public void SaveData(UserAccount user)
        {
            try
            {
                var options = new JsonSerializerOptions { WriteIndented = true };
                var json = JsonSerializer.Serialize(user, options);
                File.WriteAllText(_filePath, json);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Veri kaydedilemedi: {ex.Message}");
            }
        }

        private static UserAccount CreateSeedData()
        {
            var user = new UserAccount
            {
                CustomerNo = "8249103",
                TcNo = "12345678901",
                FullName = "Demo Müşteri",
                Pin = "1234",
                Email = "demo@cyberbank.com",
                Phone = "+90 (555) 000 00 00"
            };

            // Bank Accounts
            user.Accounts.Add(new BankAccount
            {
                Name = "Ana Vadesiz TL Hesabı",
                AccountNumber = "1005-1982491-01",
                Iban = "TR62 0006 1005 1982 4910 3001 01",
                Currency = "₺",
                CurrencyCode = "TRY",
                Balance = 84450.75m,
                AccountType = "Vadesiz TL",
                CardBackground = "#0F766E" // Teal gradient base
            });

            user.Accounts.Add(new BankAccount
            {
                Name = "Döviz Yatırım Hesabı (USD)",
                AccountNumber = "1005-1982491-02",
                Iban = "TR62 0006 1005 1982 4910 3001 02",
                Currency = "$",
                CurrencyCode = "USD",
                Balance = 3450.00m,
                AccountType = "Vadesiz Döviz",
                CardBackground = "#1E3A8A" // Blue base
            });

            user.Accounts.Add(new BankAccount
            {
                Name = "Euro Birikim Hesabı (EUR)",
                AccountNumber = "1005-1982491-03",
                Iban = "TR62 0006 1005 1982 4910 3001 03",
                Currency = "€",
                CurrencyCode = "EUR",
                Balance = 1680.00m,
                AccountType = "Vadesiz Döviz",
                CardBackground = "#4C1D95" // Purple base
            });

            user.Accounts.Add(new BankAccount
            {
                Name = "Yüksek Getirili Vadeli Mevduat (%48)",
                AccountNumber = "1005-1982491-04",
                Iban = "TR62 0006 1005 1982 4910 3001 04",
                Currency = "₺",
                CurrencyCode = "TRY",
                Balance = 150000.00m,
                AccountType = "Vadeli Mevduat (32 Gün)",
                CardBackground = "#B45309" // Amber base
            });

            // Credit Cards
            user.Cards.Add(new CreditCard
            {
                CardHolder = "DEMO KULLANICI",
                CardNumber = "5412839248109942",
                ExpiryDate = "08/29",
                Cvv = "742",
                TotalLimit = 75000m,
                AvailableLimit = 56820m,
                IsLocked = false,
                ContactlessEnabled = true,
                OnlineShoppingEnabled = true
            });

            // Saved Contacts
            user.SavedContacts.Add(new Contact
            {
                Name = "Mehmet Demir",
                Iban = "TR88 0006 2000 1122 3344 5566 77",
                BankName = "Ziraat Bankası"
            });
            user.SavedContacts.Add(new Contact
            {
                Name = "Ayşe Kaya",
                Iban = "TR12 0006 1001 9988 7766 5544 33",
                BankName = "Garanti BBVA"
            });
            user.SavedContacts.Add(new Contact
            {
                Name = "Kira - Ev Sahibi Halil",
                Iban = "TR54 0006 4000 5544 3322 1100 99",
                BankName = "İş Bankası"
            });
            user.SavedContacts.Add(new Contact
            {
                Name = "Zeynep Şahin",
                Iban = "TR32 0006 3000 8877 6655 4433 22",
                BankName = "Akbank"
            });

            // Transactions History
            var now = DateTime.Now;
            user.Transactions.Add(new Transaction
            {
                Date = now.AddHours(-2),
                Title = "FAST Transferi: Mehmet Demir",
                Description = "Yemek ortak hesabı",
                Amount = 450.00m,
                Currency = "₺",
                IsIncome = false,
                Category = "Transfer"
            });

            user.Transactions.Add(new Transaction
            {
                Date = now.AddDays(-1).AddHours(4),
                Title = "Maaş Ödemesi: Nova Teknoloji A.Ş.",
                Description = "Ağustos ayı hak ediş ödemesi",
                Amount = 62500.00m,
                Currency = "₺",
                IsIncome = true,
                Category = "Maaş"
            });

            user.Transactions.Add(new Transaction
            {
                Date = now.AddDays(-2),
                Title = "Migros Ticaret A.Ş.",
                Description = "Haftalık market alışverişi",
                Amount = 1845.30m,
                Currency = "₺",
                IsIncome = false,
                Category = "Market"
            });

            user.Transactions.Add(new Transaction
            {
                Date = now.AddDays(-3),
                Title = "Ev Kirası: Halil Bey",
                Description = "Eylül ayı kira bedeli",
                Amount = 17500.00m,
                Currency = "₺",
                IsIncome = false,
                Category = "Kira"
            });

            user.Transactions.Add(new Transaction
            {
                Date = now.AddDays(-4),
                Title = "Netflix Dijital Abonelik",
                Description = "Aylık Standart Plan",
                Amount = 229.99m,
                Currency = "₺",
                IsIncome = false,
                Category = "Abonelik"
            });

            user.Transactions.Add(new Transaction
            {
                Date = now.AddDays(-5),
                Title = "Gelen FAST: Ayşe Kaya",
                Description = "Konser bileti payı",
                Amount = 1200.00m,
                Currency = "₺",
                IsIncome = true,
                Category = "Transfer"
            });

            user.Transactions.Add(new Transaction
            {
                Date = now.AddDays(-6),
                Title = "Starbucks Coffee",
                Description = "Kadıköy şube harcaması",
                Amount = 185.00m,
                Currency = "₺",
                IsIncome = false,
                Category = "Yeme-İçme"
            });

            // Exchange Rates
            user.ExchangeRates.Add(new ExchangeRate
            {
                Code = "USD",
                Name = "Amerikan Doları",
                Symbol = "$",
                BuyingRate = 34.22m,
                SellingRate = 34.35m,
                DailyChangePercent = 0.42m
            });
            user.ExchangeRates.Add(new ExchangeRate
            {
                Code = "EUR",
                Name = "Euro",
                Symbol = "€",
                BuyingRate = 37.48m,
                SellingRate = 37.62m,
                DailyChangePercent = -0.18m
            });
            user.ExchangeRates.Add(new ExchangeRate
            {
                Code = "GA",
                Name = "Gram Altın",
                Symbol = "g",
                BuyingRate = 2835.50m,
                SellingRate = 2855.00m,
                DailyChangePercent = 1.15m
            });
            user.ExchangeRates.Add(new ExchangeRate
            {
                Code = "GBP",
                Name = "İngiliz Sterlini",
                Symbol = "£",
                BuyingRate = 44.60m,
                SellingRate = 44.85m,
                DailyChangePercent = 0.25m
            });

            // Pending Bills
            user.Bills.Add(new Bill
            {
                BillType = "Elektrik",
                Provider = "Enerjisa Dağıtım A.Ş.",
                SubscriberNo = "904128503",
                Amount = 845.50m,
                DueDate = DateTime.Now.AddDays(4).ToString("dd.MM.yyyy"),
                IsPaid = false
            });
            user.Bills.Add(new Bill
            {
                BillType = "Doğalgaz",
                Provider = "İGDAŞ İstanbul Gaz Dağıtım",
                SubscriberNo = "10492812",
                Amount = 1120.00m,
                DueDate = DateTime.Now.AddDays(7).ToString("dd.MM.yyyy"),
                IsPaid = false
            });
            user.Bills.Add(new Bill
            {
                BillType = "İnternet",
                Provider = "Türk Telekom Fiber 500Mbps",
                SubscriberNo = "702941829",
                Amount = 399.90m,
                DueDate = DateTime.Now.AddDays(2).ToString("dd.MM.yyyy"),
                IsPaid = false
            });
            user.Bills.Add(new Bill
            {
                BillType = "Su",
                Provider = "İSKİ İstanbul Su ve Kanalizasyon",
                SubscriberNo = "55019284",
                Amount = 245.20m,
                DueDate = DateTime.Now.AddDays(10).ToString("dd.MM.yyyy"),
                IsPaid = false
            });

            return user;
        }
    }
}
