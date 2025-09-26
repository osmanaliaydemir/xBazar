# xBazar API Dokümantasyonu

## Proje Genel Bakış

xBazar platformunun API servisidir. Bu proje, Domain-Driven Design (DDD) yaklaşımı ile tasarlanmış ve kullanıcı kimlik doğrulama, ürün yönetimi, sepet işlemleri, sipariş yönetimi ve ödeme işlemleri gibi temel e-ticaret fonksiyonlarını sağlar.

### Mimari Yaklaşım
- **Domain-Driven Design (DDD)**: Bounded Context ve Module yapısı
- **Repository Pattern**: Veri erişim katmanı
- **Service Layer**: Business logic katmanı
- **Application Services**: Use case'leri yöneten servisler

### Teknik Özellikler
- **Platform**: .NET 9 Web API
- **Veritabanı**: EF Core (SQL Server; plan: PostgreSQL)
- **Önbellek/Dağıtık Kilit**: Redis
- **Kimlik Doğrulama**: JWT (Bearer), API Key (opsiyonel)
- **Yetkilendirme**: RBAC + Policy-based (Permissions)
- **Hata Formatı**: RFC 7807 `application/problem+json`
- **Başarı Yanıtı**: `ApiResponse<T>` sarmalayıcı
- **Observability**: Serilog, Health Checks

---

## Kullanıcı Etkileşimleri

### 1. Kimlik Doğrulama ve Üyelik İşlemleri

#### 1.1 Kullanıcı Girişi
**Endpoint**: `POST /api/auth/login`

**Business Logic**:
- **AuthService.LoginAsync()** metodu çağrılır
- Email/username ve şifre doğrulaması yapılır
- **Password Service**: `IPasswordService.VerifyPassword()` ile şifre kontrolü
- **JWT Token**: `IJwtService.GenerateToken()` ile token oluşturulur
- **Cache**: Kullanıcı bilgileri Redis'te cache'lenir
- **Login Geçmişi**: `ISecurityEventService` ile güvenlik olayı kaydedilir
- **Rate Limiting**: IP bazlı giriş denemesi sınırlaması

**Request Body**:
```json
{
  "email": "user@example.com",
  "password": "StrongP@ssw0rd!"
}
```

**Response**:
```json
{
  "isSuccess": true,
  "data": {
    "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
    "refreshToken": "refresh_token_here",
    "expiresAt": "2025-09-24T14:34:56Z",
    "user": {
      "id": "12345678-1234-1234-1234-123456789012",
      "email": "user@example.com",
      "firstName": "John",
      "lastName": "Doe"
    }
  },
  "message": "Login successful",
  "statusCode": 200
}
```

**Özel Durumlar**:
- **Geçersiz Kimlik Bilgileri**: `401 Unauthorized` - Email veya şifre hatalı
- **Hesap Deaktif**: `403 Forbidden` - Hesap askıya alınmış
- **Rate Limiting**: `429 Too Many Requests` - Çok fazla giriş denemesi
- **Validation Error**: `400 Bad Request` - Eksik veya geçersiz alanlar

#### 1.2 Kullanıcı Kaydı
**Endpoint**: `POST /api/auth/register`

**Business Logic**:
- **UserService.CreateAsync()** metodu çağrılır
- Email ve username benzersizlik kontrolü
- **Password Service**: `IPasswordService.HashPassword()` ile şifre hash'lenir
- **Email Verification**: `IEmailService` ile doğrulama e-postası gönderilir
- **Role Assignment**: Varsayılan "User" rolü atanır
- **Cache**: Kullanıcı bilgileri cache'lenir

**Request Body**:
```json
{
  "email": "newuser@example.com",
  "userName": "newuser",
  "firstName": "Jane",
  "lastName": "Smith",
  "password": "StrongP@ssw0rd!",
  "phoneNumber": "+905551234567"
}
```

**Response**:
```json
{
  "isSuccess": true,
  "data": {
    "id": "87654321-4321-4321-4321-210987654321",
    "email": "newuser@example.com",
    "userName": "newuser",
    "firstName": "Jane",
    "lastName": "Smith",
    "emailConfirmed": false,
    "isActive": true,
    "createdAt": "2025-09-24T12:34:56Z"
  },
  "message": "User created successfully",
  "statusCode": 201
}
```

**Özel Durumlar**:
- **Email Zaten Kullanımda**: `409 Conflict` - Bu email adresi zaten kayıtlı
- **Username Zaten Kullanımda**: `409 Conflict` - Bu kullanıcı adı zaten alınmış
- **Weak Password**: `400 Bad Request` - Şifre güvenlik gereksinimlerini karşılamıyor
- **Invalid Email Format**: `400 Bad Request` - Geçersiz email formatı

#### 1.3 Token Yenileme (Refresh Token)
**Endpoint**: `POST /api/auth/refresh-token`

**Business Logic**:
- **AuthService.RefreshTokenAsync()** metodu çağrılır
- Access token'dan JWT ID (jti) claim'i alınır
- Refresh token veritabanında doğrulanır (hash kontrolü)
- Token rotation: Yeni access + refresh token üretilir
- Eski refresh token "used" olarak işaretlenir
- Token family tracking ile güvenlik sağlanır

**Request Body**:
```json
{
  "accessToken": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "refreshToken": "refresh_token_here"
}
```

**Response**:
```json
{
  "isSuccess": true,
  "data": {
    "accessToken": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
    "refreshToken": "new_refresh_token_here",
    "expiresAt": "2025-09-24T14:34:56Z",
    "user": {
      "id": "12345678-1234-1234-1234-123456789012",
      "email": "user@example.com",
      "userName": "user",
      "firstName": "John",
      "lastName": "Doe",
      "emailConfirmed": true,
      "roles": ["User"]
    }
  },
  "message": "Token refreshed successfully",
  "statusCode": 200
}
```

**Cookie Desteği**:
- Refresh token otomatik olarak HttpOnly cookie'ye kaydedilir
- Cookie: `refresh_token` (HttpOnly, Secure, SameSite=Strict)
- Süre: 7 gün (yapılandırılabilir)

**Özel Durumlar**:
- **Invalid Access Token**: `401 Unauthorized` - Geçersiz veya süresi dolmuş access token
- **Invalid Refresh Token**: `401 Unauthorized` - Geçersiz refresh token
- **Token Expired**: `401 Unauthorized` - Refresh token süresi dolmuş
- **Token Reuse Detected**: `401 Unauthorized` - Aynı refresh token tekrar kullanılmış (güvenlik ihlali)
- **User Not Found**: `401 Unauthorized` - Kullanıcı bulunamadı veya deaktif

#### 1.4 Çıkış (Logout)
**Endpoint**: `POST /api/auth/logout`

**Business Logic**:
- **AuthService.LogoutAsync()** metodu çağrılır
- Refresh token veritabanında iptal edilir
- HttpOnly cookie temizlenir
- Token family güvenliği sağlanır

**Request Body**:
```json
{
  "refreshToken": "refresh_token_here"
}
```

**Response**:
```json
{
  "isSuccess": true,
  "data": true,
  "message": "Logout successful",
  "statusCode": 200
}
```

**Özel Durumlar**:
- **Invalid Refresh Token**: `400 Bad Request` - Geçersiz refresh token

---

## Kimlik Doğrulama Detayları

### JWT Bearer Token
- Authorization header: `Authorization: Bearer <token>`
- Token içinde `sub` (Guid) claim'i kullanıcı kimliği olarak kullanılır
- **JWT ID (jti)**: Her token'ın benzersiz kimliği
- **Access Token süresi**: 15 dakika (yapılandırılabilir)
- **Refresh Token süresi**: 7 gün (yapılandırılabilir)
- **Token Rotation**: Her refresh işleminde yeni token çifti üretilir
- **Database Storage**: Refresh token'lar veritabanında hash'lenerek saklanır

### API Key (Opsiyonel)
- Header: `X-Api-Key: <key>`
- Kapsam kontrolü endpoint bazlı uygulanabilir
- Rate limiting ve logging için kullanılır

## Yetkilendirme (RBAC & Policies)
- Roller ve izinler `Core.Constants.Roles` ve `Core.Constants.Permissions` altında tanımlıdır.
- Controller/actions üzerinde `[Authorize(Policy = "Users_Read")]` gibi policy kullanılır.
- Mağaza bazlı yetkiler için Store resource handler devrededir.

Örnek Policy Kullanımı:
```csharp
[Authorize(Policy = "Users_Read")]
public async Task<IActionResult> GetAll(...)
```

## Standart Başarı Yanıtı: ApiResponse<T>
```json
{
  "isSuccess": true,
  "data": { "id": "...", "name": "..." },
  "message": "optional",
  "errors": [],
  "correlationId": "...",
  "timestamp": "2025-09-24T12:34:56Z",
  "statusCode": 200
}
```

### Yardımcılar
- `ApiResponse.Success(data, message?, statusCode?)`
- `ApiResponse.NotFound(message)`
- `ApiResponse.BadRequest(message, errors?)`
- `ApiResponse.Unauthorized(message)`
- `ApiResponse.Forbidden(message)`
- `ApiResponse.Conflict(message)`

## Hata Sözleşmesi: RFC 7807 Problem Details
Tüm hatalar `application/problem+json` içerik türünde döner.

Örnek (Not Found):
```json
{
  "type": "about:blank",
  "title": "Not Found",
  "status": 404,
  "detail": "User not found",
  "instance": "/api/users/00000000-0000-0000-0000-000000000000"
}
```

Örnek (Validation Error):
```json
{
  "type": "about:blank",
  "title": "Validation Error",
  "status": 400,
  "detail": "One or more validation errors occurred.",
  "extensions": {
    "errors": [
      "Email is required",
      "Password must be at least 8 characters"
    ]
  }
}
```

Desteklenen domain hataları (`Core.Exceptions`):
- `NotFoundException` → 404
- `ConflictException` → 409
- `ValidationException` → 400 (+extensions.errors)
- `ForbiddenException` → 403
- `DomainException` → Özelleştirilebilir StatusCode

## Soft Delete ve Admin Görünürlüğü
- Global query filter: `IsDeleted = false`
- Admin/StoreOwner/StoreManager silinmiş kayıtları geçici olarak görebilir.
- `ISoftDeleteService.DisableSoftDeleteFilter<T>()` ile filtre devre dışı bırakılır.

Örnek (Servis içinde):
```csharp
using var _ = _softDeleteService.DisableSoftDeleteFilter<User>();
var user = await _unitOfWork.Users.GetByIdAsync(id);
```

## Sepet Eşzamanlılık (ETag & Dağıtık Kilit)
- ETag/Version alanları ile optimistik eşzamanlılık.
- Redis tabanlı dağıtık kilit ile Guest → User sepet birleşimi yarış koşulları önlenir.
- ETag uyumsuzluğunda `409 Conflict` döner.

İstek Başlığı:
```
If-Match: "<cart-etag>"
```

Yanıt Başlığı:
```
ETag: "<new-etag>"
```

## Güvenlik Başlıkları ve Oran Sınırlama
- Güvenlik başlıkları middleware üzerinden uygulanır (HSTS, CORS yapılandırması önerilir).
- Rate limiting in-memory; dağıtık için geliştirme planı mevcuttur.

### CSRF Koruması
- **CSRF Protection Middleware** aktif
- State-changing operasyonlar (POST, PUT, PATCH, DELETE) için CSRF token gerekli
- **Header**: `X-XSRF-TOKEN: <token>`
- **Cookie**: `XSRF-TOKEN` (JavaScript erişilebilir)
- **Güvenli Endpoint'ler**: GET, HEAD, OPTIONS ve auth endpoint'leri hariç

## Swagger/OpenAPI
- `ApiResponseOperationFilter` ile başarı ve hata örnekleri eklenir.
- `application/json` ve `application/problem+json` içerik türleri desteklenir.

---

### 2. Kullanıcı Yönetimi

#### 2.1 Kullanıcı Listesi
**Endpoint**: `GET /api/users?page=1&pageSize=20`

**Business Logic**:
- **UserService.GetAllAsync()** metodu çağrılır
- Sayfalama ile kullanıcı listesi getirilir
- Soft delete filtresi uygulanır (admin değilse)
- **Cache**: Kullanıcı listesi Redis'te cache'lenir

**Headers**:
```http
Authorization: Bearer <token>
```

**Response**:
```json
{
  "isSuccess": true,
  "data": {
    "items": [
      {
        "id": "12345678-1234-1234-1234-123456789012",
        "email": "user1@example.com",
        "userName": "user1",
        "firstName": "John",
        "lastName": "Doe",
        "isActive": true,
        "createdAt": "2025-09-24T12:34:56Z"
      }
    ],
    "totalCount": 100,
    "page": 1,
    "pageSize": 20,
    "totalPages": 5,
    "hasNextPage": true,
    "hasPreviousPage": false
  },
  "statusCode": 200
}
```

**Özel Durumlar**:
- **Yetkisiz Erişim**: `403 Forbidden` - Users_Read permission gerekli
- **Geçersiz Sayfa**: `400 Bad Request` - Page veya pageSize geçersiz

#### 2.2 Kullanıcı Detayı
**Endpoint**: `GET /api/users/{id}`

**Business Logic**:
- **UserService.GetByIdAsync()** metodu çağrılır
- Kullanıcı ID ile detay getirilir
- Admin ise silinmiş kullanıcıları da görebilir
- **Cache**: Kullanıcı detayı Redis'te cache'lenir

**Headers**:
```http
Authorization: Bearer <token>
```

**Response**:
```json
{
  "isSuccess": true,
  "data": {
    "id": "12345678-1234-1234-1234-123456789012",
    "email": "user@example.com",
    "userName": "user",
    "firstName": "John",
    "lastName": "Doe",
    "phoneNumber": "+905551234567",
    "emailConfirmed": true,
    "isActive": true,
    "roles": [
      {
        "id": "role-id-1",
        "name": "User"
      }
    ],
    "createdAt": "2025-09-24T12:34:56Z",
    "updatedAt": "2025-09-24T12:34:56Z"
  },
  "statusCode": 200
}
```

**Özel Durumlar**:
- **Kullanıcı Bulunamadı**: `404 Not Found` - Belirtilen ID'de kullanıcı yok
- **Yetkisiz Erişim**: `403 Forbidden` - Users_Read permission gerekli

#### 2.3 Kullanıcı Oluşturma
**Endpoint**: `POST /api/users`

**Business Logic**:
- **UserService.CreateAsync()** metodu çağrılır
- Email ve username benzersizlik kontrolü
- **Password Service**: Şifre hash'lenir
- **Role Assignment**: Belirtilen roller atanır
- **Email Service**: Doğrulama e-postası gönderilir
- **Cache**: Yeni kullanıcı cache'lenir

**Headers**:
```http
Authorization: Bearer <token>
Content-Type: application/json
```

**Request Body**:
```json
{
  "email": "newuser@example.com",
  "userName": "newuser",
  "firstName": "Jane",
  "lastName": "Smith",
  "password": "StrongP@ssw0rd!",
  "phoneNumber": "+905551234567",
  "roleIds": ["role-id-1", "role-id-2"]
}
```

**Response**:
```json
{
  "isSuccess": true,
  "data": {
    "id": "87654321-4321-4321-4321-210987654321",
    "email": "newuser@example.com",
    "userName": "newuser",
    "firstName": "Jane",
    "lastName": "Smith",
    "phoneNumber": "+905551234567",
    "emailConfirmed": false,
    "isActive": true,
    "roles": [
      {
        "id": "role-id-1",
        "name": "User"
      }
    ],
    "createdAt": "2025-09-24T12:34:56Z"
  },
  "message": "User created successfully",
  "statusCode": 201
}
```

**Özel Durumlar**:
- **Email Zaten Kullanımda**: `409 Conflict` - Bu email adresi zaten kayıtlı
- **Username Zaten Kullanımda**: `409 Conflict` - Bu kullanıcı adı zaten alınmış
- **Geçersiz Role ID**: `400 Bad Request` - Belirtilen rol bulunamadı
- **Yetkisiz Erişim**: `403 Forbidden` - Users_Write permission gerekli

#### 2.4 Kullanıcı Güncelleme
**Endpoint**: `PUT /api/users/{id}`

**Business Logic**:
- **UserService.UpdateAsync()** metodu çağrılır
- Kullanıcı varlık kontrolü
- Email/username benzersizlik kontrolü (kendisi hariç)
- **Role Update**: Mevcut roller silinir, yenileri atanır
- **Cache**: Güncellenmiş kullanıcı cache'lenir

**Headers**:
```http
Authorization: Bearer <token>
Content-Type: application/json
```

**Request Body**:
```json
{
  "email": "updated@example.com",
  "userName": "updateduser",
  "firstName": "Updated",
  "lastName": "User",
  "phoneNumber": "+905559876543",
  "isActive": true,
  "roleIds": ["role-id-1"]
}
```

**Response**:
```json
{
  "isSuccess": true,
  "data": {
    "id": "12345678-1234-1234-1234-123456789012",
    "email": "updated@example.com",
    "userName": "updateduser",
    "firstName": "Updated",
    "lastName": "User",
    "phoneNumber": "+905559876543",
    "isActive": true,
    "roles": [
      {
        "id": "role-id-1",
        "name": "User"
      }
    ],
    "updatedAt": "2025-09-24T12:34:56Z"
  },
  "message": "User updated successfully",
  "statusCode": 200
}
```

**Özel Durumlar**:
- **Kullanıcı Bulunamadı**: `404 Not Found` - Belirtilen ID'de kullanıcı yok
- **Email Zaten Kullanımda**: `409 Conflict` - Bu email başka kullanıcı tarafından kullanılıyor
- **Yetkisiz Erişim**: `403 Forbidden` - Users_Write permission gerekli

#### 2.5 Kullanıcı Silme (Soft Delete)
**Endpoint**: `DELETE /api/users/{id}`

**Business Logic**:
- **UserService.DeleteAsync()** metodu çağrılır
- Kullanıcı varlık kontrolü
- **Soft Delete**: `IsDeleted = true` olarak işaretlenir
- **Cache**: Kullanıcı cache'den kaldırılır
- **Audit Log**: Silme işlemi loglanır

**Headers**:
```http
Authorization: Bearer <token>
```

**Response**:
```json
{
  "isSuccess": true,
  "data": true,
  "message": "User deleted successfully",
  "statusCode": 200
}
```

**Özel Durumlar**:
- **Kullanıcı Bulunamadı**: `404 Not Found` - Belirtilen ID'de kullanıcı yok
- **Yetkisiz Erişim**: `403 Forbidden` - Users_Delete permission gerekli

### 3. Ürün Yönetimi

#### 3.1 Ürün Detayı
**Endpoint**: `GET /api/products/{id}`

**Business Logic**:
- **ProductService.GetByIdAsync()** metodu çağrılır
- Ürün ID ile detay getirilir
- Soft delete filtresi uygulanır
- **Cache**: Ürün detayı Redis'te cache'lenir

**Response**:
```json
{
  "isSuccess": true,
  "data": {
    "id": "product-id-123",
    "name": "Samsung Galaxy S24",
    "sku": "SAM-GAL-S24-256",
    "description": "Latest Samsung smartphone",
    "unitPrice": 25000.00,
    "stockQuantity": 50,
    "isActive": true,
    "store": {
      "id": "store-id-1",
      "name": "TechStore"
    },
    "category": {
      "id": "cat-id-1",
      "name": "Smartphones"
    },
    "images": [
      {
        "url": "https://example.com/images/galaxy-s24.jpg",
        "isPrimary": true
      }
    ],
    "createdAt": "2025-09-24T12:34:56Z"
  },
  "statusCode": 200
}
```

**Özel Durumlar**:
- **Ürün Bulunamadı**: `404 Not Found` - Belirtilen ID'de ürün yok
- **Ürün Deaktif**: `404 Not Found` - Ürün aktif değil

### 4. Sepet Yönetimi

#### 4.1 Sepete Ürün Ekleme
**Endpoint**: `POST /api/cart/items`

**Business Logic**:
- **CartService.AddItemAsync()** metodu çağrılır
- Ürün varlık ve stok kontrolü
- **Product Service**: Ürün detayları getirilir
- **Tax Service**: Vergi hesaplaması yapılır
- **Shipping Service**: Kargo hesaplaması yapılır
- **ETag**: Yeni ETag oluşturulur
- **Cache**: Sepet Redis'te güncellenir

**Headers**:
```http
Content-Type: application/json
```

**Request Body**:
```json
{
  "productId": "product-id-123",
  "quantity": 2
}
```

**Response**:
```json
{
  "isSuccess": true,
  "data": {
    "sessionId": "guest-session-123",
    "userId": null,
    "items": [
      {
        "productId": "product-id-123",
        "productName": "Samsung Galaxy S24",
        "sku": "SAM-GAL-S24-256",
        "unitPrice": 25000.00,
        "quantity": 2,
        "totalPrice": 50000.00
      }
    ],
    "subTotal": 50000.00,
    "taxAmount": 9000.00,
    "shippingAmount": 25.00,
    "discountAmount": 0.00,
    "totalAmount": 59025.00,
    "couponCode": null,
    "couponDiscount": null,
    "createdAt": "2025-09-24T12:34:56Z",
    "updatedAt": "2025-09-24T12:34:56Z",
    "eTag": "\"f5d8eeef\"",
    "version": 1
  },
  "statusCode": 200
}
```

**Response Headers**:
```http
ETag: "f5d8eeef"
```

**Özel Durumlar**:
- **Ürün Bulunamadı**: `404 Not Found` - Belirtilen ürün bulunamadı
- **Yetersiz Stok**: `400 Bad Request` - İstenen miktar stokta yok
- **Geçersiz Miktar**: `400 Bad Request` - Miktar 0'dan büyük olmalı

#### 4.2 Sepet Öğesi Güncelleme (ETag ile)
**Endpoint**: `PUT /api/cart/items/{productId}`

**Business Logic**:
- **CartService.UpdateItemWithETagAsync()** metodu çağrılır
- ETag kontrolü ile eşzamanlılık kontrolü
- Ürün varlık ve stok kontrolü
- Sepet yeniden hesaplanır
- Yeni ETag oluşturulur

**Headers**:
```http
Content-Type: application/json
If-Match: "f5d8eeef"
```

**Request Body**:
```json
{
  "quantity": 3
}
```

**Response**:
```json
{
  "isSuccess": true,
  "data": {
    "sessionId": "guest-session-123",
    "items": [
      {
        "productId": "product-id-123",
        "productName": "Samsung Galaxy S24",
        "sku": "SAM-GAL-S24-256",
        "unitPrice": 25000.00,
        "quantity": 3,
        "totalPrice": 75000.00
      }
    ],
    "subTotal": 75000.00,
    "taxAmount": 13500.00,
    "shippingAmount": 25.00,
    "totalAmount": 88525.00,
    "eTag": "\"a1b2c3d4\"",
    "version": 2
  },
  "statusCode": 200
}
```

**Response Headers**:
```http
ETag: "a1b2c3d4"
```

**Özel Durumlar**:
- **ETag Uyuşmazlığı**: `409 Conflict` - Sepet başka bir işlem tarafından değiştirilmiş
- **Ürün Bulunamadı**: `404 Not Found` - Sepette böyle bir ürün yok
- **Yetersiz Stok**: `400 Bad Request` - İstenen miktar stokta yok

#### 4.3 Guest → User Sepet Birleştirme
**Endpoint**: `POST /api/cart/merge`

**Business Logic**:
- **CartService.MergeCartsAsync()** metodu çağrılır
- **Distributed Lock**: Redis tabanlı kilit ile yarış koşulları önlenir
- Guest sepeti ile user sepeti birleştirilir
- Aynı ürünler için miktarlar toplanır
- Sepet yeniden hesaplanır
- Guest sepeti temizlenir

**Headers**:
```http
Authorization: Bearer <token>
Content-Type: application/json
```

**Request Body**:
```json
{
  "sessionId": "guest-session-123"
}
```

**Response**:
```json
{
  "isSuccess": true,
  "data": true,
  "message": "Carts merged successfully",
  "statusCode": 200
}
```

**Özel Durumlar**:
- **Kilit Alınamadı**: `409 Conflict` - Sepet başka bir işlem tarafından kullanılıyor
- **User Bulunamadı**: `404 Not Found` - Token'daki user bulunamadı
- **Guest Sepet Bulunamadı**: `404 Not Found` - Belirtilen session'da sepet yok

### 5. Sipariş Yönetimi

#### 5.1 Sipariş Oluşturma
**Endpoint**: `POST /api/orders`

**Business Logic**:
- **OrderService.CreateAsync()** metodu çağrılır
- Kullanıcı ve mağaza varlık kontrolü
- Ürün varlık ve stok kontrolü
- Sipariş numarası oluşturulur
- Sipariş öğeleri oluşturulur
- **Payment Service**: Ödeme işlemi başlatılır

**Headers**:
```http
Content-Type: application/json
```

**Request Body**:
```json
{
  "userId": "user-id-123",
  "storeId": "store-id-1",
  "orderItems": [
    {
      "productId": "product-id-123",
      "quantity": 1,
      "unitPrice": 25000.00
    }
  ],
  "shippingAddress": {
    "firstName": "John",
    "lastName": "Doe",
    "address": "123 Main St",
    "city": "Istanbul",
    "postalCode": "34000"
  }
}
```

**Response**:
```json
{
  "isSuccess": true,
  "data": {
    "id": "order-id-123",
    "orderNumber": "ORD-2025-001234",
    "userId": "user-id-123",
    "storeId": "store-id-1",
    "status": "New",
    "subTotal": 25000.00,
    "taxAmount": 4500.00,
    "shippingAmount": 25.00,
    "totalAmount": 29525.00,
    "orderItems": [
      {
        "productId": "product-id-123",
        "productName": "Samsung Galaxy S24",
        "quantity": 1,
        "unitPrice": 25000.00,
        "totalPrice": 25000.00
      }
    ],
    "createdAt": "2025-09-24T12:34:56Z"
  },
  "message": "Order created successfully",
  "statusCode": 201
}
```

**Özel Durumlar**:
- **Ürün Bulunamadı**: `404 Not Found` - Belirtilen ürün bulunamadı
- **Yetersiz Stok**: `400 Bad Request` - Ürün stokta yok
- **Mağaza Bulunamadı**: `404 Not Found` - Belirtilen mağaza bulunamadı

### 6. Mesajlaşma

#### 6.1 Thread Mesajları
**Endpoint**: `GET /api/messages/threads/{threadId}/messages?page=1&pageSize=50`

**Business Logic**:
- **MessagingService.GetThreadMessagesAsync()** metodu çağrılır
- Thread varlık kontrolü
- Kullanıcı thread'e katılımcı mı kontrolü
- Sayfalama ile mesajlar getirilir

**Headers**:
```http
Authorization: Bearer <token>
```

**Response**:
```json
{
  "isSuccess": true,
  "data": [
    {
      "id": "message-id-1",
      "threadId": "thread-id-123",
      "senderId": "user-id-1",
      "receiverId": "user-id-2",
      "content": "Merhaba, siparişim hakkında soru sormak istiyorum.",
      "type": "Text",
      "isRead": false,
      "createdAt": "2025-09-24T12:34:56Z"
    }
  ],
  "statusCode": 200
}
```

**Özel Durumlar**:
- **Thread Bulunamadı**: `404 Not Found` - Belirtilen thread bulunamadı
- **Yetkisiz Erişim**: `403 Forbidden` - Bu thread'e erişim yetkiniz yok

---

## Health Checks
```http
GET /health
```
- SQL, Redis, dış servis bağlantı kontrolleri

## Hata Kodları ve Haritalama
- 400: `ValidationException`
- 401: Kimlik doğrulama başarısız (middleware/policy)
- 403: `ForbiddenException`, policy ihlali
- 404: `NotFoundException`
- 409: `ConflictException` (ör. SKU çakışması)
- 429: Rate limiting (plan)
- 500: Beklenmeyen hatalar

## En İyi Pratikler
- Idempotency için uygun yerlerde `Idempotency-Key` kullanın (Ödeme/Webhook)
- ETag ile optimistik eşzamanlılık uygulayın
- Policy tabanlı yetkilendirme kullanın, rollere doğrudan bağlanmayın
- Problem+json ile hataları standartlaştırın

### Refresh Token Güvenlik Özellikleri
- **Token Rotation**: Her refresh işleminde yeni token çifti üretilir
- **Reuse Detection**: Aynı refresh token tekrar kullanılırsa tüm token ailesi iptal edilir
- **Database Storage**: Token'lar hash'lenerek veritabanında saklanır
- **HttpOnly Cookie**: XSS saldırılarına karşı koruma
- **SameSite=Strict**: CSRF saldırılarına karşı koruma
- **JWT ID Tracking**: Her access token'ın benzersiz kimliği takip edilir
- **Token Family Revocation**: Güvenlik ihlali durumunda tüm token'lar iptal edilir

---

Bu dokümantasyon, Swagger UI ile birlikte referans amaçlıdır. Swagger üzerinde örnekler ve şemalar otomatik olarak üretilir (başarı ve hata yanıtları dahil).
