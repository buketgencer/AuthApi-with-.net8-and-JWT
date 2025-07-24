# AuthApi-with-.net8-and-JWT

# 🏥 AuthApi - JWT Authentication & BMI Calculator API

Modern ve güvenli bir ASP.NET Core 8 Web API projesi. Bu proje, JWT tabanlı kimlik doğrulama sistemi ve BMI (Vücut Kitle İndeksi) hesaplama özellikleri sunar.

## 📋 İçindekiler

- [Özellikler](#-özellikler)
- [Kullanılan Teknolojiler](#-kullanılan-teknolojiler)
- [Kurulum](#-kurulum)
- [Yapılandırma](#-yapılandırma)
- [Veritabanı](#-veritabanı)
- [API Endpoints](#-api-endpoints)
- [Kimlik Doğrulama](#-kimlik-doğrulama)
- [Roller ve Yetkiler](#-roller-ve-yetkiler)
- [BMI Hesaplama](#-bmi-hesaplama)
- [Swagger Dokümantasyonu](#-swagger-dokümantasyonu)
- [Test Senaryoları](#-test-senaryoları)
- [Hata Yönetimi](#-hata-yönetimi)
- [Geliştirici Notları](#-geliştirici-notları)
- [Katkıda Bulunma](#-katkıda-bulunma)
- [Lisans](#-lisans)

## ✨ Özellikler

### 🔐 **Kimlik Doğrulama & Yetkilendirme**
- JWT Token tabanlı kimlik doğrulama
- Kullanıcı kayıt ve giriş sistemi
- Rol tabanlı yetkilendirme (Admin/User)
- Güvenli şifre hashleme (BCrypt)
- Token süre dolumu ve yenileme

### 👤 **Kullanıcı Yönetimi**
- Kullanıcı profil yönetimi
- Aktivasyon/deaktivasyon sistemi
- Son giriş tarihi takibi
- Email ve username benzersizlik kontrolü

### 📊 **BMI (Vücut Kitle İndeksi) Sistemi**
- Kişisel BMI hesaplama ve kaydetme
- BMI kategorileri (Zayıf, Normal, Fazla Kilolu, Obez vb.)
- Sağlık önerileri
- Admin paneli için BMI istatistikleri
- Cinsiyet bazlı değerlendirme

### 📈 **Admin Paneli**
- Kullanıcı istatistikleri
- Tüm kullanıcıların BMI verilerini görüntüleme
- BMI kategori dağılım istatistikleri
- Sistem genel durumu

### 🛡️ **Güvenlik**
- CORS yapılandırması
- Input validation
- SQL Injection koruması
- XSS koruması
- Rate limiting (opsiyonel)

## 🛠 Kullanılan Teknolojiler

- **Framework:** ASP.NET Core 8.0
- **Database:** SQLite (Entity Framework Core)
- **Authentication:** JWT Bearer Token
- **Password Hashing:** BCrypt.Net
- **ORM:** Entity Framework Core 8.0
- **API Documentation:** Swagger/OpenAPI
- **Logging:** ASP.NET Core Logging
- **Validation:** Data Annotations

### 📦 NuGet Paketleri

```xml
<PackageReference Include="BCrypt.Net-Next" Version="4.0.3" />
<PackageReference Include="Microsoft.AspNetCore.Authentication.JwtBearer" Version="8.0.5" />
<PackageReference Include="Microsoft.EntityFrameworkCore.Sqlite" Version="8.0.5" />
<PackageReference Include="Microsoft.EntityFrameworkCore.Tools" Version="8.0.5" />
<PackageReference Include="Microsoft.EntityFrameworkCore.Design" Version="8.0.5" />
<PackageReference Include="Microsoft.IdentityModel.Tokens" Version="7.1.2" />
<PackageReference Include="Swashbuckle.AspNetCore" Version="6.4.0" />
```

## 🚀 Kurulum

### Ön Gereksinimler

- .NET 8.0 SDK
- Visual Studio 2022 / VS Code / JetBrains Rider
- Git

### Adım 1: Projeyi Klonlayın

```bash
git clone https://github.com/yourusername/AuthApi.git
cd AuthApi
```

### Adım 2: Bağımlılıkları Yükleyin

```bash
dotnet restore
```

### Adım 3: Veritabanını Oluşturun

```bash
# Entity Framework Core Tools'u yükleyin (eğer yüklü değilse)
dotnet tool install --global dotnet-ef

# Migration'ları çalıştırın
dotnet ef migrations add InitialCreate
dotnet ef database update
```

### Adım 4: Uygulamayı Çalıştırın

```bash
dotnet run
```

Uygulama varsayılan olarak şu adreslerde çalışacaktır:
- **HTTP:** http://localhost:5000
- **HTTPS:** https://localhost:5001
- **Swagger:** https://localhost:5001/swagger

## ⚙️ Yapılandırma

### appsettings.json

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "AllowedHosts": "*",
  "ConnectionStrings": {
    "DefaultConnection": "Data Source=auth.db"
  },
  "Jwt": {
    "Key": "MyVerySecretJwtKey2024ForDevelopmentPurposesOnly123456789",
    "Issuer": "AuthApi",
    "Audience": "AuthApiUsers",
    "ExpiryHours": "24"
  },
  "Cors": {
    "AllowedOrigins": ["http://localhost:3000", "https://localhost:3001"]
  }
}
```

### JWT Konfigürasyonu

- **Key:** JWT imzalama için kullanılan gizli anahtar (production'da mutlaka değiştirin!)
- **Issuer:** Token'ı kim yayınladı
- **Audience:** Token'ı kim kullanacak
- **ExpiryHours:** Token'ın geçerlilik süresi (saat cinsinden)

## 🗄️ Veritabanı

### Tablo Yapısı

#### Users Tablosu

| Sütun | Tip | Açıklama |
|-------|------|----------|
| Id | INTEGER | Primary Key, Auto Increment |
| Username | TEXT(50) | Benzersiz kullanıcı adı |
| Email | TEXT(100) | Benzersiz email adresi |
| PasswordHash | TEXT | BCrypt ile hashlenmiş şifre |
| Role | TEXT(20) | Kullanıcı rolü (Admin/User) |
| CreatedAt | DATETIME | Oluşturulma tarihi |
| LastLoginAt | DATETIME | Son giriş tarihi |
| IsActive | BOOLEAN | Hesap aktiflik durumu |

#### BmiData Tablosu

| Sütun | Tip | Açıklama |
|-------|------|----------|
| Id | INTEGER | Primary Key, Auto Increment |
| UserId | INTEGER | Foreign Key (Users.Id) |
| Height | DECIMAL(5,2) | Boy (metre cinsinden) |
| Weight | DECIMAL(6,2) | Kilo (kg cinsinden) |
| Gender | TEXT(10) | Cinsiyet (Erkek/Kadın) |
| BmiValue | DECIMAL(5,2) | Hesaplanan BMI değeri |
| BmiCategory | TEXT(20) | BMI kategorisi |
| CreatedAt | DATETIME | Oluşturulma tarihi |
| UpdatedAt | DATETIME | Güncellenme tarihi |

### Migration Komutları

```bash
# Yeni migration oluştur
dotnet ef migrations add MigrationName

# Veritabanını güncelle
dotnet ef database update

# Migration'ları listele
dotnet ef migrations list

# Son migration'ı geri al
dotnet ef migrations remove

# Belirli bir migration'a geri dön
dotnet ef database update MigrationName
```

## 🌐 API Endpoints

### 🔐 Authentication Endpoints

#### POST /api/auth/register
Yeni kullanıcı kaydı

**Request Body:**
```json
{
  "username": "johndoe",
  "email": "john@example.com",
  "password": "123456",
  "confirmPassword": "123456",
  "role": "User"  // Opsiyonel, default: "User"
}
```

**Response:**
```json
{
  "success": true,
  "message": "Kullanıcı başarıyla kaydedildi.",
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "expiresAt": "2024-07-25T00:00:00Z",
  "user": {
    "id": 1,
    "username": "johndoe",
    "email": "john@example.com",
    "role": "User",
    "createdAt": "2024-07-24T00:00:00Z",
    "lastLoginAt": null
  }
}
```

#### POST /api/auth/login
Kullanıcı girişi

**Request Body:**
```json
{
  "email": "john@example.com",
  "password": "123456"
}
```

**Response:**
```json
{
  "success": true,
  "message": "Hoşgeldin johndoe!",
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "expiresAt": "2024-07-25T00:00:00Z",
  "user": {
    "id": 1,
    "username": "johndoe",
    "email": "john@example.com",
    "role": "User",
    "createdAt": "2024-07-24T00:00:00Z",
    "lastLoginAt": "2024-07-24T10:30:00Z"
  }
}
```

#### GET /api/auth/profile
Kullanıcı profil bilgileri (🔒 Token gerekli)

**Headers:**
```
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
```

**Response:**
```json
{
  "success": true,
  "message": "Profil bilgileri alındı.",
  "user": {
    "id": 1,
    "username": "johndoe",
    "email": "john@example.com",
    "role": "User",
    "createdAt": "2024-07-24T00:00:00Z",
    "lastLoginAt": "2024-07-24T10:30:00Z"
  }
}
```

#### GET /api/auth/test
Test endpoint (Herkese açık)

**Response:**
```json
{
  "success": true,
  "message": "AuthApi çalışıyor!",
  "timestamp": "2024-07-24T10:30:00Z",
  "version": "1.0.0"
}
```

#### GET /api/auth/admin
Admin panel istatistikleri (🔒 Sadece Admin)

**Response:**
```json
{
  "success": true,
  "message": "Admin panel verisi",
  "data": {
    "statistics": {
      "totalUsers": 25,
      "activeUsers": 23,
      "adminUsers": 2,
      "userUsers": 23
    },
    "recentUsers": [
      {
        "id": 25,
        "username": "newuser",
        "email": "new@example.com",
        "role": "User",
        "createdAt": "2024-07-24T09:00:00Z",
        "lastLoginAt": null
      }
    ]
  }
}
```

### 📊 BMI Endpoints

#### POST /api/bmi/calculate
BMI hesaplama ve kaydetme (🔒 Token gerekli)

**Request Body:**
```json
{
  "height": 1.75,      // metre cinsinden
  "weight": 70.5,      // kg cinsinden
  "gender": "Erkek"    // "Erkek" veya "Kadın"
}
```

**Response:**
```json
{
  "success": true,
  "message": "BMI başarıyla hesaplandı ve kaydedildi.",
  "data": {
    "id": 1,
    "height": 1.75,
    "weight": 70.5,
    "gender": "Erkek",
    "bmiValue": 23.02,
    "bmiCategory": "Normal",
    "advice": "İdeal kilodasınız! Bu durumu korumaya devam edin.",
    "createdAt": "2024-07-24T10:30:00Z",
    "updatedAt": null
  }
}
```

#### GET /api/bmi/my-bmi
Kişisel BMI bilgileri (🔒 Token gerekli)

**Response:**
```json
{
  "success": true,
  "message": "BMI bilgileri getirildi.",
  "data": {
    "id": 1,
    "height": 1.75,
    "weight": 70.5,
    "gender": "Erkek",
    "bmiValue": 23.02,
    "bmiCategory": "Normal",
    "advice": "İdeal kilodasınız! Bu durumu korumaya devam edin.",
    "createdAt": "2024-07-24T10:30:00Z",
    "updatedAt": "2024-07-24T11:00:00Z"
  }
}
```

#### GET /api/bmi/all-users-bmi
Tüm kullanıcıların BMI bilgileri (🔒 Sadece Admin)

**Response:**
```json
{
  "success": true,
  "message": "Toplam 5 kullanıcının BMI bilgileri getirildi.",
  "data": [
    {
      "id": 1,
      "userId": 1,
      "username": "johndoe",
      "email": "john@example.com",
      "height": 1.75,
      "weight": 70.5,
      "gender": "Erkek",
      "bmiValue": 23.02,
      "bmiCategory": "Normal",
      "createdAt": "2024-07-24T10:30:00Z",
      "updatedAt": null
    }
  ]
}
```

#### GET /api/bmi/statistics
BMI istatistikleri (🔒 Sadece Admin)

**Response:**
```json
{
  "success": true,
  "message": "BMI istatistikleri getirildi.",
  "data": {
    "totalRecords": 50,
    "genderDistribution": {
      "male": 28,
      "female": 22,
      "malePercentage": 56.0,
      "femalePercentage": 44.0
    },
    "categoryDistribution": [
      {
        "category": "Normal",
        "count": 25,
        "percentage": 50.0
      },
      {
        "category": "Fazla Kilolu",
        "count": 15,
        "percentage": 30.0
      }
    ],
    "averageBmi": 24.85
  }
}
```

## 🔐 Kimlik Doğrulama

### JWT Token Kullanımı

1. **Kayıt/Giriş:** `/api/auth/register` veya `/api/auth/login` endpoint'ini kullanarak token alın
2. **Token Kullanımı:** Her korumalı endpoint için `Authorization` header'ına token ekleyin

```bash
# Örnek cURL komutu
curl -X GET "https://localhost:5001/api/auth/profile" \
  -H "Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9..."
```

### Token Payload

JWT token aşağıdaki bilgileri içerir:

```json
{
  "user_id": "1",
  "username": "johndoe",
  "email": "john@example.com",
  "role": "User",
  "sub": "1",
  "jti": "unique-jwt-id",
  "iat": 1690200000,
  "exp": 1690286400,
  "nbf": 1690200000,
  "iss": "AuthApi",
  "aud": "AuthApiUsers"
}
```

## 👥 Roller ve Yetkiler

### Kullanıcı Rolleri

#### 👤 User (Varsayılan)
- Kendi hesabına kayıt olabilir
- Giriş yapabilir
- Kendi profil bilgilerini görüntüleyebilir
- BMI hesaplama yapabilir
- Kendi BMI geçmişini görüntüleyebilir

#### 👑 Admin
- User rolünün tüm yetkileri
- Tüm kullanıcıları görüntüleyebilir
- BMI istatistiklerini görüntüleyebilir
- Tüm kullanıcıların BMI verilerini görüntüleyebilir
- Sistem geneli raporları alabilir

### Yetkilendirme Attribute'ları

```csharp
[Authorize] // Sadece giriş yapmış kullanıcılar
[Authorize(Roles = UserRole.Admin)] // Sadece Admin'ler
[Authorize(Roles = UserRole.User)] // Sadece User'lar
[AllowAnonymous] // Herkese açık
```

## 📊 BMI Hesaplama

### BMI Formülü

```
BMI = Kilo (kg) / (Boy (m))²
```

### BMI Kategorileri

| BMI Değeri | Kategori | Açıklama |
|------------|----------|----------|
| < 18.5 | Zayıf | Kilo almanız önerilir |
| 18.5 - 24.9 | Normal | İdeal kilodasınız |
| 25.0 - 29.9 | Fazla Kilolu | Kilo vermeniz önerilir |
| 30.0 - 34.9 | Obez (1. Derece) | Uzman görüşü alın |
| 35.0 - 39.9 | Obez (2. Derece) | Ciddi obezite riski |
| ≥ 40.0 | Morbid Obez | Acil tıbbi müdahale |

### BMI Hesaplama Özellikleri

- **Otomatik Hesaplama:** Boy ve kilo bilgileri girildikten sonra BMI otomatik hesaplanır
- **Kategori Belirleme:** BMI değerine göre otomatik kategori ataması
- **Sağlık Önerileri:** Her kategori için özelleştirilmiş sağlık tavsiyeleri
- **Cinsiyet Bazlı Değerlendirme:** Erkek/Kadın için farklı öneriler
- **Güncellenebilir Kayıtlar:** Kullanıcı verilerini güncelleyebilir
- **Geçmiş Takibi:** BMI değişimlerinin takibi

## 📚 Swagger Dokümantasyonu

Swagger UI'ya erişim için uygulamayı çalıştırdıktan sonra:

**URL:** https://localhost:5001/swagger

### Swagger Özellikleri

- **Interactive API Testing:** Doğrudan tarayıcıdan API testleri
- **JWT Authorization:** Swagger üzerinden token ile test yapma
- **Request/Response Examples:** Örnek istek ve yanıtlar
- **Schema Documentation:** Veri modellerinin detaylı dokümantasyonu

### Swagger'da JWT Token Kullanımı

1. `/api/auth/login` endpoint'ini kullanarak token alın
2. Swagger sayfasının sağ üst köşesindeki **"Authorize"** butonuna tıklayın
3. Token'ı `Bearer <your_token>` formatında girin
4. Artık korumalı endpoint'leri test edebilirsiniz

## 🧪 Test Senaryoları

### Postman Collection

```json
{
  "info": {
    "name": "AuthApi Tests",
    "schema": "https://schema.getpostman.com/json/collection/v2.1.0/collection.json"
  },
  "auth": {
    "type": "bearer",
    "bearer": [
      {
        "key": "token",
        "value": "{{jwt_token}}",
        "type": "string"
      }
    ]
  },
  "event": [
    {
      "listen": "prerequest",
      "script": {
        "exec": [
          "// Auto-refresh token if needed"
        ]
      }
    }
  ]
}
```

### Test Kullanıcıları

```sql
-- Admin kullanıcı
INSERT INTO Users (Username, Email, PasswordHash, Role, CreatedAt, IsActive)
VALUES ('admin', 'admin@example.com', '$2a$11$hashedpassword', 'Admin', datetime('now'), 1);

-- Normal kullanıcı
INSERT INTO Users (Username, Email, PasswordHash, Role, CreatedAt, IsActive)
VALUES ('user', 'user@example.com', '$2a$11$hashedpassword', 'User', datetime('now'), 1);
```

### Örnek Test Senaryoları

#### ✅ Pozitif Test Senaryoları

1. **Kullanıcı Kaydı**
   - Valid email, username, password ile kayıt
   - Başarılı token alımı

2. **Kullanıcı Girişi**
   - Doğru email/password ile giriş
   - Token alımı ve profil erişimi

3. **BMI Hesaplama**
   - Valid boy/kilo değerleri ile BMI hesaplama
   - Kategori ve öneri alımı

4. **Admin İşlemleri**
   - Admin user ile kullanıcı listesi görüntüleme
   - BMI istatistikleri görüntüleme

#### ❌ Negatif Test Senaryoları

1. **Geçersiz Kayıt**
   - Zaten var olan email ile kayıt
   - Zayıf şifre ile kayıt
   - Geçersiz email formatı

2. **Geçersiz Giriş**
   - Yanlış email/password
   - Deaktif kullanıcı ile giriş

3. **Yetkilendirme Hataları**
   - Token olmadan korumalı endpoint erişimi
   - Süresi dolmuş token kullanımı
   - User rolü ile admin endpoint erişimi

4. **Geçersiz BMI Verisi**
   - Negatif boy/kilo değerleri
   - Aşırı yüksek değerler
   - Geçersiz cinsiyet

## ⚠️ Hata Yönetimi

### HTTP Status Kodları

| Status Code | Açıklama | Örnek |
|-------------|----------|-------|
| 200 | Başarılı | Veri alımı |
| 201 | Oluşturuldu | Yeni kayıt |
| 400 | Bad Request | Geçersiz veri |
| 401 | Unauthorized | Token yok/geçersiz |
| 403 | Forbidden | Yetki yetersiz |
| 404 | Not Found | Veri bulunamadı |
| 409 | Conflict | Veri çakışması |
| 500 | Server Error | Sunucu hatası |

### Hata Response Formatı

```json
{
  "success": false,
  "message": "Hata açıklaması",
  "errors": ["Detaylı hata 1", "Detaylı hata 2"],
  "timestamp": "2024-07-24T10:30:00Z"
}
```

### Yaygın Hatalar ve Çözümleri

#### 401 Unauthorized
```json
{
  "success": false,
  "message": "Geçersiz token."
}
```
**Çözüm:** Token'ınızın geçerli olduğundan ve `Bearer ` prefix'i ile gönderildiğinden emin olun.

#### 400 Bad Request - Validation Errors
```json
{
  "success": false,
  "message": "Doğrulama hatası: Email zorunludur, Şifre en az 6 karakter olmalıdır"
}
```
**Çözüm:** İstek verinizi kontrol edin ve gerekli alanları doldurun.

#### 409 Conflict
```json
{
  "success": false,
  "message": "Bu email adresi zaten kayıtlı."
}
```
**Çözüm:** Farklı bir email adresi kullanın.

## 🔧 Geliştirici Notları

### Proje Yapısı

```
AuthApi/
├── Controllers/          # API Controller'ları
│   ├── AuthController.cs
│   └── BmiController.cs
├── Data/                # Database Context
│   └── AuthDbContext.cs
├── DTOs/                # Data Transfer Objects
│   ├── AuthResponse.cs
│   ├── BmiRequest.cs
│   ├── BmiResponse.cs
│   ├── LoginRequest.cs
│   └── RegisterRequest.cs
├── Models/              # Entity Models
│   ├── BmiData.cs
│   ├── User.cs
│   └── UserRole.cs
├── Services/            # Business Logic
│   └── JwtService.cs
├── Migrations/          # EF Core Migrations
├── Properties/
│   └── launchSettings.json
├── appsettings.json     # Configuration
├── Program.cs           # Application Entry Point
└── AuthApi.csproj       # Project File
```

### Clean Architecture Principles

- **Controllers:** HTTP isteklerini karşılar, minimum business logic
- **Services:** Business logic ve domain rules
- **DTOs:** API contract'ları, validation rules
- **Models:** Database entities
- **Data:** Database access layer

### Coding Standards

- **Naming:** PascalCase for public members, camelCase for private
- **Comments:** XML documentation for public APIs
- **Error Handling:** Try-catch blocks with proper logging
- **Validation:** Data Annotations + manual validation
- **Security:** Input sanitization, SQL injection prevention

### Performance Considerations

- **Database Indexes:** Username ve Email unique indexes
- **Caching:** JWT token'lar client-side cache edilebilir
- **Pagination:** Büyük veri setleri için sayfalama
- **Connection Pooling:** EF Core otomatik yönetir

### Security Best Practices

1. **JWT Secret:** Production'da güçlü, rastgele key kullanın
2. **HTTPS:** Sadece HTTPS üzerinden erişim
3. **CORS:** Sadece güvenilir domain'lere izin
4. **Input Validation:** Tüm kullanıcı girdilerini validate edin
5. **Rate Limiting:** API abuse'i önlemek için
6. **Logging:** Güvenlik olaylarını kaydedin

### Environment Configurations

#### Development
```json
{
  "Jwt": {
    "Key": "DevelopmentKeyMinimum32Characters",
    "ExpiryHours": "24"
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information"
    }
  }
}
```

#### Production
```json
{
  "Jwt": {
    "Key": "ProductionSuperSecretKey256BitLong",
    "ExpiryHours": "8"
  },
  "Logging": {
    "LogLevel": {
      "Default": "Warning"
    }
  }
}
```

### Deplo

- [ ] JWT Key production değeri
- [ ] Database connection string
- [ ] CORS ayarları
- [ ] Logging konfigürasyonu
- [ ] SSL/TLS sertifikaları
- [ ] Environment variables
- [ ] Database migrations

## 🤝 Katkıda Bulunma

1. Fork yapın
2. Feature branch oluşturun (`git checkout -b feature/amazing-feature`)
3. Değişikliklerinizi commit edin (`git commit -m 'Add amazing feature'`)
4. Branch'inizi push edin (`git push origin feature/amazing-feature`)
5. Pull Request oluşturun

### Geliştirme Rehberi

- Kod standartlarına uyun
- Unit testler yazın
- Commit mesajlarını açıklayıcı yazın
- Documentation güncelleyin

## 📝 Lisans

Bu proje MIT lisansı altında lisanslanmıştır. Detaylar için `LICENSE` dosyasına bakın.

## 📞 İletişim

- **Proje Sahibi:** [Buket Gencer]
- **Email:** gncrbuket@gmail.com

## 🙏 Teşekkürler

Bu projeyi geliştirirken kullanılan açık kaynak kütüphaneler:

- ASP.NET Core Team
- Entity Framework Team
- BCrypt.Net Contributors
- JWT Contributors
- Swagger/OpenAPI Contributors

---

**⭐ Bu projeyi beğendiyseniz, lütfen yıldız verin!**

## 📈 Logging

Projede ASP.NET Core'un built-in logging sistemi kullanılmaktadır:

```csharp
_logger.LogInformation($"Yeni kullanıcı kaydedildi: {user.Username}");
_logger.LogError(ex, "Kullanıcı kaydı sırasında hata oluştu");
```

### Log Seviyeleri

- **Information:** Normal işlem logları
- **Warning:** Uyarı mesajları  
- **Error:** Hata logları
- **Debug:** Development ortamı için detaylı loglar

## 🔄 Gelecek Geliştirmeler

Proje için planlanan özellikler:

- [ ] Şifre sıfırlama sistemi
- [ ] Email doğrulama
- [ ] Kullanıcı profil fotoğrafı
- [ ] BMI geçmiş grafiği
- [ ] Hedef kilo belirleme
- [ ] API rate limiting
- [ ] Redis cache entegrasyonu
- [ ] Unit testler
- [ ] Integration testler
