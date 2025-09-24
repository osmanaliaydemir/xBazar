# xBazar - Çok Satıcılı Pazaryeri Sistemi

## 🎯 PROJE HEDEFİ
Modern, ölçeklenebilir çok satıcılı e-ticaret platformu. Mağazalar kendi ürünlerini satabilir, müşteriler alışveriş yapabilir, admin tüm sistemi yönetebilir.

## 🏗️ TEKNİK MİMARİ
- **Backend**: .NET 9 WebAPI + Worker Services
- **Frontend**: Razor  + TypeScript
- **Database**: PostgreSQL (EF Core) + Redis (Cache/Sepet)
- **Search**: PostgreSQL Full-Text Search (başlangıçta)
- **Message Queue**: RabbitMQ (async işlemler)
- **Storage**: MinIO (S3 uyumlu)
- **Monitoring**: Serilog + OpenTelemetry + Prometheus

## 👥 KULLANICI ROLLERİ

### 1. MİSAFİR (Guest)
- Üyelik olmadan alışveriş
- Sepete ekleme/çıkarma
- Adres bilgisi girme
- Kargo seçimi
- Ödeme yapma
- Sipariş takibi (sipariş numarası ile)

### 2. MÜŞTERİ (Customer)
- Hesap açma/giriş/çıkış
- Profil yönetimi
- Adres defteri
- Kayıtlı kartlar
- Sipariş geçmişi
- Favori ürünler/mağazalar
- Mağaza takip etme
- Mağaza ile mesajlaşma
- İade talepleri
- Şifre değiştirme

### 3. MAĞAZA SAHİBİ (Store Owner)
- Mağaza başvurusu
- Mağaza profil yönetimi
- Ürün yönetimi (CRUD)
- Stok yönetimi
- Sipariş yönetimi
- Kargo işlemleri
- Tahsilat görüntüleme
- Mağaza istatistikleri
- Müşteri mesajlarına yanıt

### 4. MAĞAZA YETKİLİSİ (Store Staff)
- Owner tarafından yetkilendirilmiş işlemler
- Sipariş işleme
- Kargo hazırlama
- Müşteri desteği

### 5. ADMIN
- Tüm sisteme tam erişim
- Mağaza onay/ret
- Kullanıcı yönetimi
- Sipariş müdahalesi
- Ödeme işlemleri
- Tahsilat yönetimi
- Raporlar ve analitik
- Sistem ayarları

##️ MODÜLLER

### 1. KULLANICI YÖNETİMİ
- Kayıt/oturum/çıkış
- E-posta doğrulama
- Şifre sıfırlama
- Profil yönetimi
- Adres defteri
- Kayıtlı kartlar

### 2. MAĞAZA YÖNETİMİ
- Mağaza başvuru formu
- KYC süreçleri
- Mağaza profil yönetimi
- Mağaza sayfası oluşturma
- İç yetkilendirme (RBAC)
- Müşteri mesajlarına yanıt

### 3. ÜRÜN KATALOĞU
- Kategori yönetimi
- Ürün CRUD işlemleri
- Stok yönetimi
- Fiyat yönetimi
- Ürün arama/filtreleme
- Favori ürünler

### 4. SEPET & CHECKOUT
- Sepet yönetimi (Redis)
- Adres seçimi
- Kargo seçenekleri
- Ödeme işlemleri (tek PSP - İyzico)
- Sipariş oluşturma

### 5. SİPARİŞ YÖNETİMİ
- Sipariş durum takibi
- Kargo işlemleri
- Teslimat onayı
- İade süreçleri
- Sipariş raporları

### 6. ÖDEME & TAHSİLAT
- İyzico entegrasyonu
- Tahsilat yönetimi
- Komisyon hesaplama
- Payout işlemleri

### 7. KUPON & KAMPANYA
- Kupon oluşturma
- Kampanya yönetimi
- İndirim hesaplama
- Kullanım takibi

### 8. MESAJLAŞMA
- Müşteri ↔ Mağaza
- Dosya ekleme
- Mesaj geçmişi

### 9. RAPORLAMA
- Satış raporları
- Sipariş analitikleri
- Mağaza performansı
- Admin dashboard

## 🔒 GÜVENLİK DETAYLARI

### Kimlik Doğrulama & Yetkilendirme
- JWT token tabanlı authentication
- Refresh token mekanizması
- Role-based access control (RBAC)
- Policy-based authorization
- Session management
- Password hashing (bcrypt)

### API Güvenliği
- Rate limiting (IP/kullanıcı bazlı)
- CORS konfigürasyonu
- API key validation
- Request size limiting
- SQL injection koruması
- XSS koruması

### Veri Güvenliği
- Input validation (FluentValidation)
- Output encoding
- File upload güvenliği
- Sensitive data encryption
- Database connection string encryption
- Environment variables güvenliği

### Network Güvenliği
- HTTPS zorunluluğu
- HSTS headers
- Security headers (CSP, X-Frame-Options)
- Anti-forgery tokens
- Secure cookies

## 🚨 ERROR HANDLING & LOGGING

### Global Exception Handling
- Centralized exception handling middleware
- Custom exception types
- Error response standardization
- Error correlation IDs
- Stack trace sanitization

### Logging Stratejisi
- Structured logging (Serilog)
- Log levels (Debug, Info, Warning, Error, Fatal)
- Correlation ID tracking
- Request/Response logging
- Performance logging
- Security event logging

### Error Tracking
- Sentry entegrasyonu
- Error aggregation
- Alert notifications
- Error trend analysis
- Performance monitoring

### Health Checks
- Database connectivity
- Redis connectivity
- External service health
- Custom health indicators
- Liveness/Readiness probes

## ⚖️ COMPLIANCE & LEGAL

### KVKK/GDPR Uyumluluğu
- Veri işleme faaliyetleri kaydı
- Kullanıcı onay yönetimi
- Veri silme hakkı (Right to be forgotten)
- Veri taşınabilirlik hakkı
- Veri ihlali bildirimi
- Privacy policy yönetimi

### E-Ticaret Yasa Uyumluluğu
- Mesafeli satış sözleşmesi
- Tüketici hakları
- İade/iptal süreçleri
- Fatura düzenleme
- Vergi mükellefiyeti takibi

### Veri Saklama Politikaları
- Kullanıcı verileri saklama süreleri
- Sipariş verileri saklama süreleri
- Log verileri saklama süreleri
- Otomatik veri silme işlemleri
- Arşivleme stratejileri

### Audit Trail
- Kullanıcı işlem logları
- Sistem değişiklik logları
- Güvenlik olay logları
- Compliance raporları
- Denetim desteği

## VERİ MODELİ

### Ana Tablolar
- Users (kullanıcılar)
- Stores (mağazalar)
- Products (ürünler)
- Orders (siparişler)
- OrderItems (sipariş kalemleri)
- Payments (ödeme)
- Coupons (kuponlar)
- Messages (mesajlar)
- Favorites (favoriler)
- AuditLogs (denetim logları)

## 🧪 TEST STRATEJİSİ
- Unit Testler (her modül için)
- Integration Testler (API endpoint'leri)
- E2E Testler (kritik akışlar)
- Security Testler (penetration testing)
- Performance Testler (yük testi)
- Compliance Testler (KVKK/GDPR)

## DEPLOYMENT
- Docker containerization
- docker-compose (local development)
- GitHub Actions (CI/CD)
- Environment variables yönetimi
- Secrets management
- Blue-green deployment

## GELİŞTİRME AŞAMALARI

### Aşama 1: Temel Altyapı (2 hafta)
- Proje yapısı oluşturma
- Database tasarımı
- Temel API endpoint'leri
- Authentication/Authorization
- Güvenlik middleware'leri

### Aşama 2: Kullanıcı & Mağaza Yönetimi (2 hafta)
- Kullanıcı kayıt/giriş
- Mağaza başvuru süreci
- Mağaza profil yönetimi
- RBAC implementasyonu

### Aşama 3: Ürün & Katalog (2 hafta)
- Ürün CRUD işlemleri
- Kategori yönetimi
- Arama/filtreleme
- Input validation

### Aşama 4: Sepet & Checkout (2 hafta)
- Sepet yönetimi
- Checkout süreci
- İyzico entegrasyonu
- Error handling

### Aşama 5: Sipariş & Kargo (2 hafta)
- Sipariş yönetimi
- Kargo işlemleri
- Durum takibi
- Logging implementasyonu

### Aşama 6: İleri Özellikler (2 hafta)
- Mesajlaşma
- Favoriler
- Kupon/kampanya
- Raporlama

### Aşama 7: Admin Panel (1 hafta)
- Admin dashboard
- Sistem yönetimi
- Raporlar
- Audit trail

### Aşama 8: Compliance & Test (1 hafta)
- KVKK/GDPR implementasyonu
- Test yazma
- Security testing
- Performance optimizasyonu

## 📝 NOTLAR
- Her aşamada test yazılacak
- Her aşamada dokümantasyon güncellenecek
- Her aşamada code review yapılacak
- Her aşamada security review yapılacak
- Her aşamada compliance kontrolü yapılacak
- Her aşamada deployment test edilecek

## 🚀 BAŞLANGIÇ KOMUTLARI
```bash
# Proje oluşturma
dotnet new sln -n xBazar
dotnet new webapi -n xBazar.API
dotnet new classlib -n xBazar.Domain
dotnet new classlib -n xBazar.Application
dotnet new classlib -n xBazar.Infrastructure

# Frontend oluşturma
npx create-next-app@latest xBazar.Web --typescript --tailwind --app

# Docker compose
docker-compose up -d
```
