# 🛒 xBazar - E-Ticaret Platformu

Modern, ölçeklenebilir ve güvenli bir e-ticaret platformu. .NET 9, PostgreSQL, Redis ve Blazor teknolojileri ile geliştirilmiştir.

## 🚀 Özellikler

### **Backend (API)**
- **.NET 9 WebAPI** - Yüksek performanslı RESTful API
- **PostgreSQL** - Güçlü veritabanı yönetimi
- **Redis** - Cache ve distributed locking
- **JWT Authentication** - Güvenli kimlik doğrulama
- **RBAC Authorization** - Rol bazlı erişim kontrolü
- **Soft Delete** - Veri güvenliği
- **ETag/Concurrency** - Optimistic concurrency control
- **Health Checks** - Sistem durumu izleme
- **Swagger/OpenAPI** - API dokümantasyonu

### **Frontend (Web)**
- **Blazor Web App** - Unified render mode
- **SSR + WASM** - Vitrin için SEO dostu
- **Interactive Server** - Mağaza/Admin panelleri
- **MudBlazor** - Modern UI bileşenleri
- **Responsive Design** - Mobil uyumlu

## 🏗️ Mimari

### **Clean Architecture**
```
src/
├── Core/           # Domain entities, interfaces, exceptions
├── Application/    # Business logic, services, DTOs
├── Infrastructure/ # Data access, external services
└── API/           # Web API controllers, middleware
```

### **Teknoloji Stack**
- **Backend**: .NET 9, ASP.NET Core, Entity Framework Core
- **Database**: PostgreSQL, Redis
- **Frontend**: Blazor Web App, MudBlazor
- **Authentication**: JWT, OAuth2
- **Logging**: Serilog
- **Documentation**: Swagger/OpenAPI

## 🚀 Hızlı Başlangıç

### **Gereksinimler**
- .NET 9 SDK
- PostgreSQL 15+
- Redis 7+
- Visual Studio 2022 / VS Code

### **Kurulum**

1. **Repository'yi klonla**
```bash
git clone https://github.com/osmanaliaydemir/xBazar.git
cd xBazar
```

2. **Veritabanını ayarla**
```bash
# PostgreSQL connection string'i appsettings.json'da güncelle
# Redis connection string'i appsettings.json'da güncelle
```

3. **Migration'ları çalıştır**
```bash
cd src/API
dotnet ef database update
```

4. **Projeyi çalıştır**
```bash
# API
cd src/API
dotnet run

# Web (gelecekte)
cd src/Web
dotnet run
```

## 📚 API Dokümantasyonu

Detaylı API dokümantasyonu için [docs/API.md](docs/API.md) dosyasına bakın.

## 🔧 Geliştirme

### **Proje Yapısı**
- **Core**: Domain entities, interfaces, exceptions
- **Application**: Business logic, services, DTOs
- **Infrastructure**: Data access, external services
- **API**: Web API controllers, middleware

### **Kod Standartları**
- SOLID prensipleri
- Clean Code
- Domain-Driven Design
- Repository Pattern
- CQRS (Command Query Responsibility Segregation)

## 🛡️ Güvenlik

- JWT token-based authentication
- Role-based access control (RBAC)
- Policy-based authorization
- SQL injection koruması
- XSS koruması
- Rate limiting
- Security headers

## 📊 Performans

- Redis cache
- Distributed locking
- Optimistic concurrency control
- Health checks
- Logging ve monitoring

## 🤝 Katkıda Bulunma

1. Fork yapın
2. Feature branch oluşturun (`git checkout -b feature/amazing-feature`)
3. Commit yapın (`git commit -m 'Add amazing feature'`)
4. Push yapın (`git push origin feature/amazing-feature`)
5. Pull Request oluşturun

## 📄 Lisans

Bu proje MIT lisansı altında lisanslanmıştır. Detaylar için [LICENSE](LICENSE) dosyasına bakın.

## 👨‍💻 Geliştirici

**Osman Ali Aydemir**
- .NET Fullstack Developer
- Software Architect
- [GitHub](https://github.com/osmanaliaydemir)

## 📞 İletişim

Proje hakkında sorularınız için [GitHub Issues](https://github.com/osmanaliaydemir/xBazar/issues) kullanın.

---

⭐ Bu projeyi beğendiyseniz yıldız vermeyi unutmayın!
