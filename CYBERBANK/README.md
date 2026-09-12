# CyberBank - Masaüstü Bankacılık Simülasyonu 🛡️💳

CyberBank, **C#** ve **.NET 10 WPF** kullanılarak geliştirilmiş, modern fintech arayüzüne sahip yeni nesil bir masaüstü dijital bankacılık simülasyonudur.

---

## 🌟 Özellikler

- **Modern Fintech Arayüzü:** Koyu tema (Dark Mode), gradyan kart efektleri ve akıcı geçişler.
- **Güvenli Kimlik Doğrulama:** PIN / Müşteri No doğrulaması ve demo hesaba tek tıkla hızlı giriş.
- **Finansal Gösterge Paneli (Dashboard):** Vadesiz TL, Döviz ve Vadeli Mevduat hesapları ile toplam varlık özeti.
- **Görsel Platinum Kredi Kartı:** Limit kullanımı, dönem borcu takibi, tek tıkla borç ödeme ve kart numarasını gizleme/gösterme.
- **Dijital ATM Simülasyonu:** Hesaba nakit yatırma ve çekme işlemleri.
- **7/24 FAST & Havale Transferleri:** Kayıtlı alıcılar, hızlı tutar butonları ve referans numaralı dijital dekont.
- **Kart Güvenlik Yönetimi:** Kartı geçici olarak kilitleme, E-Ticaret yetkisi aç/kapat, temassız (NFC) ödeme aç/kapat, limit güncelleme.
- **Detaylı Hesap Hareketleri:** Harcama kategorileri, anlık arama motoru, gelir/gider analizi.
- **Döviz & Altın Portföyü:** Canlı kur tablosu, kur çevirici ve anında döviz alım-satımı.
- **Fatura Ödeme:** Elektrik, su, doğalgaz, internet faturaları ve kurum borç sorgulama.
- **Kalıcı Veri (Local Storage):** Yapılan tüm işlemler yerel JSON veritabanında saklanır.

---

## 🛠️ Teknolojiler

- **Dil:** C# (.NET 10.0 Windows Desktop)
- **Arayüz Framework:** WPF (Windows Presentation Foundation) & XAML
- **Mimari:** MVVM / Servis Tabanlı Mimari
- **Veri Depolama:** Yerel JSON Depolama (`System.Text.Json`)

---

## 🚀 Kurulum ve Çalıştırma

Projeyi Visual Studio, VS Code Insiders veya doğrudan .NET CLI ile çalıştırabilirsiniz.

### Gereksinimler
- [.NET 10 SDK](https://dotnet.microsoft.com/) veya .NET 8 SDK
- Visual Studio 2022 / VS Code / VS Code Insiders

### Komut Satırından Çalıştırma:
```bash
# Bağımlılıkları geri yükleyin ve derleyin
dotnet build

# Uygulamayı başlatın
dotnet run
```

### Visual Studio Code / VS Code Insiders ile:
1. Proje klasörünü VS Code ile açın.
2. `F5` tuşuna basarak uygulamayı hata ayıklama modunda başlatın.

---

## 🔒 Güvenlik & Gizlilik Notu
Bu uygulama bir **simülasyon** projesidir. İçerisinde yer alan tüm IBAN numaraları, kart bilgileri, müşteri numaraları ve işlem kayıtları kurgusaldır. Hiçbir gerçek banka servisi veya kişisel veri kullanılmamaktadır.
