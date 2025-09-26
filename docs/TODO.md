# TODO (xBazar)

## ✅ **Son Güncellemeler (Aralık 2024)**

### **JWT Refresh Token Mimarisi Tamamlandı**
- ✅ Veritabanı tabanlı refresh token yönetimi (RefreshToken entity)
- ✅ JWT ID (jti) claim desteği ve token tracking
- ✅ CSRF Protection Middleware eklendi
- ✅ Token rotation ve reuse detection iyileştirildi
- ✅ HttpOnly cookie desteği
- ✅ Token family revocation (tekrar kullanımda tüm aile iptal)

### **Karar: SQL Server'da Kalınacak**
- ❌ PostgreSQL geçişi iptal edildi
- ✅ Mevcut SQL Server altyapısı korunacak
- ✅ Database First yaklaşımı devam edecek

## Mimari Sapmalar ve Altyapı Eksikleri
- [x] Veritabanı motorunu Postgres'e geçir (plan: PostgreSQL; mevcut: SQL Server) - **İPTAL EDİLDİ: SQL Server'da kalınacak**
  - [x] `ApplicationDbContext` sağlayıcısını Npgsql'e çevir - **İPTAL EDİLDİ**
  - [x] Connection string ve provider bağımlılıklarını güncelle (`Npgsql.EntityFrameworkCore.PostgreSQL`) - **İPTAL EDİLDİ**
  - [x] Migration'ları yeniden oluştur ve veri taşıma stratejisini yaz - **İPTAL EDİLDİ**
- [ ] Redis yapılandırmasını ortama göre parametrize et (SSL/Password, sentinel/cluster desteği)
- [ ] Mesaj kuyruğu (RabbitMQ) ekle (şu an yok)
  - [ ] Bağlantı, publisher, consumer altyapısı ve örnek iş (email, audit offload)
- [ ] Dosya saklama MinIO/S3 entegrasyonu (şu an local `wwwroot/uploads`)
  - [ ] `IFileStorageService` için S3 uyumlu implementasyon ve presigned URL'ler
- [ ] Gözlemlenebilirlik: OpenTelemetry + Prometheus ekle (şu an sadece Serilog)
  - [ ] Trace, metrics, log enrichment; Prometheus endpoint

## Güvenlik ve Kimlik Doğrulama
- [x] Refresh token mimarisini tamamla ve belgeyle
  - [x] `Jwt` ayarlarında access/refresh sürelerini tutarlı adlarla kullan (code vs config mismatch)
  - [x] Token aile rotasyonu, reuse detection (kısmen var) için kalıcı blacklisting ve TTL
  - [x] Refresh token'ı HttpOnly cookie olarak ver ve yenileme endpointinde CSRF koruması ekle
  - [x] **YENİ**: Veritabanı tabanlı refresh token yönetimi (RefreshToken entity)
  - [x] **YENİ**: JWT ID (jti) claim desteği ve token tracking
  - [x] **YENİ**: CSRF Protection Middleware eklendi
  - [x] **YENİ**: Token rotation ve reuse detection iyileştirildi
- [x] RBAC/Policy-based authorization kuralları (policy'ler tanımlanmamış)
  - [x] `Permissions` sabitlerine karşı policy registration
  - [x] Controller/action bazlı `[Authorize(Policy="...")]` uygulaması
- [ ] Data Protection key ring kalıcı hale getir (Redis/FS) – çok instance senaryosu
- [ ] Rate limiting dağıtık hale getir (şu an in-memory)
- [ ] API Key doğrulamada kapsam kontrolü
  - [ ] `IsApiKeyRequired`/skip list mantığını attribute veya endpoint metadata ile netleştir
- [ ] CORS yapılandırmasını ortama göre sıkılaştır (AllowCredentials kullanımı, origin listesi)
- [ ] HSTS/HTTPS enforcement üretimde doğrula (security headers gözden geçir)

## Ödeme ve Checkout
- [ ] Iyzico gerçek entegrasyonu (şu an mock `PaymentService`)
  - [ ] Auth/signature, 3DS akışı, status polling, hata haritalama
  - [ ] Webhook endpoint'leri ve imza doğrulama
  - [ ] Idempotency key deposu (şu an cache ile kısmi)
- [ ] `PaymentOptions` ve `FileStorageOptions` için `Options` binding aç (DI'da yorum satırı)
- [ ] `CheckoutService` için tüm doğrulamalar ve stok/rezervasyon entegrasyonu

## Sepet ve Cache
- [x] Sepette fiyat, stok, vergi, kargo hesaplamalarını gerçek servislere bağla (şu an placeholder)
- [ ] Kupon/indirim hesaplaması (şu an 0/placeholder)
- [x] Guest → User sepet birleştirmede yarış koşullarına karşı kilitleme/ETag
- [ ] Redis key pattern delete fonksiyonunda çoklu endpoint sunucu seçimi ve scan kullanımını gözden geçir

## Arama ve Raporlama
- [ ] Full-Text Search (plan) – Postgres FTS ile ürün arama/filtreleme
- [ ] Raporlama ve metrik toplama; admin dashboard API'leri henüz eksik

## Sağlık, Logging ve Audit
- [ ] Health checks: ödeme servisinin gerçek uç noktası ve timeouts
- [ ] Yapısal loglarda PII maskesi (kısmen var, alan seti gözden geçir)
- [ ] Audit trail: kapsam (hangi entity alanları) ve performans; bulk insert/async kuyruk opsiyonu

## Uygulama Katmanı ve API
- [ ] Tüm modüller için FluentValidation kapsamını genişlet (sadece bazı DTO'lar var)
- [x] Controller'larda tutarlı `ApiResponse` dönüşleri ve hata sözleşmesi
- [x] Global exception çevirileri (domain → http) ve problem+json
- [ ] Dosya yükleme sınırları/mime doğrulama ve antivirüs tarama entegrasyonu

## Konfigürasyon ve Ortamlar
- [ ] `appsettings` sırlarının Secret Manager/Environment ile taşınması (mail, payment keys)
- [ ] Production `appsettings.Production.json` ve container env mapping
- [ ] CI/CD: build, test, migration apply ve health gate adımları

## Veri Modeli ve Migrasyonlar
- [ ] Migration klasörü boş – mevcut entity'lerle migration oluştur
- [ ] Index/constraint'ler: plan dosyasındaki index ve FKs ile uyum
- [x] Soft delete filtrelerinin iş kurallarıyla etkileşimi (admin görünürlüğü)

## Worker Servisi
- [ ] Worker’da kuyruk tüketicileri, zamanlanmış işler (rapor, temizleme, e-posta)
- [ ] Retry ve dead-letter stratejisi

## Belgeler ve Test
- [x] API belge standardizasyonu (Swagger açıklamaları, örnekler, hata kodları)
- [ ] Unit/Integration test kapsamı: Auth, Cart, Checkout, Orders, Payments
- [ ] Güvenlik testleri (rate limit, api key bypass, CSRF, SSRF, file upload)
- [ ] Performans testleri; sepet/checkout yük senaryoları

---

## Hızlı Aksiyon Listesi (Önceliklendirilmiş)
1) ~~Postgres'e geçiş ve migration'lar~~ **İPTAL EDİLDİ: SQL Server'da kalınacak**
2) **Gerçek Iyzico entegrasyonu + webhook** ⭐ **ÖNCELİK**
3) ~~Policy-based RBAC ve permission enforcement~~ **TAMAMLANDI**
4) **Dağıtık rate limiting + data protection key ring** ⭐ **YÜKSEK ÖNCELİK**
5) **MinIO dosya saklama + presigned upload** ⭐ **YÜKSEK ÖNCELİK**
6) **OpenTelemetry + Prometheus** ⭐ **ORTA ÖNCELİK**
7) Sepet fiyat/kupon/vergilendirme gerçek servislere bağlama
8) CI/CD pipeline ve prod konfigürasyonları
