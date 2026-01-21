# 📌 Talep360 Proje Dokümantasyonu (PRD)

Bu belge, **Talep360** projesinin güncel durumunu, mimari kararlarını, tamamlanan özellikleri ve teknik detaylarını içermektedir. Takım arkadaşlarının projenin mevcut durumuna hızlıca adapte olabilmesi için hazırlanmıştır.

---

## 🚀 1. Proje Özeti ve Amacı
**Talep360**, kurumsal ihtiyaçlar için tasarlanmış modern bir **Talep Yönetim Sistemi (Ticket System)** projesidir. Kullanıcıların talepler oluşturabildiği, yöneticilerin bu talepleri ilgili personellere atayabildiği ve süreçlerin takip edilebildiği bir mikroservis mimarisi üzerine kuruludur.

**Temel Hedef:** Ölçeklenebilir, güvenli ve modüler bir yapı ile talep süreçlerini dijitalleştirmek.

---

## 🏗 2. Mimari Yapı (Microservices)

Proje, **.NET 8** tabanlı mikroservis mimarisi ile geliştirilmektedir. Servisler arası iletişim ve dış dünyaya açılma işlemleri bir **API Gateway** üzerinden yönetilmektedir.

### 🧩 Servisler
1.  **OcelotGateway (API Gateway):**
    *   Tüm dış isteklerin karşılandığı tek giriş noktasıdır.
    *   İstekleri ilgili servislere yönlendirir (Routing).
    *   Authentication (JWT) doğrulamasını yönetir.

2.  **TalepAuthentication (Identity Service):**
    *   Kullanıcı Kayıt (Register) ve Giriş (Login) işlemleri.
    *   JWT Token üretimi.
    *   Rol Yönetimi (Admin, Manager, Staff, User).
    *   Kullanıcı yetkilendirme ve listeleme işlemleri.

3.  **TalepService (Ticket Service):**
    *   Talep (Ticket) oluşturma, güncelleme, silme ve listeleme.
    *   Dosya eki (Attachment) yönetimi.
    *   İş yükü analizi (Workload Stats).
    *   İş mantığı ve durum yönetimi (State Machine).

---

## 🛠 3. Teknoloji Yığını (Tech Stack)

*   **Framework:** .NET 8 (Core)
*   **Veritabanı:** MS SQL Server (Entity Framework Core - Code First)
*   **API Gateway:** Ocelot
*   **Kimlik Doğrulama:** JWT (JSON Web Token) + ASP.NET Core Identity
*   **Validasyon:** FluentValidation
*   **API Dokümantasyonu:** Swagger / OpenAPI
*   **Mimarisi Desenleri:**
    *   Repository Pattern (Generic)
    *   Service Layer
    *   Dependency Injection (DI)
    *   DTO (Data Transfer Object) Pattern
    *   Unit of Work / Transaction Management

---

## ✅ 4. Tamamlanan Özellikler ve Geliştirmeler

### 🔐 TalepAuthentication Servisi
*   **Kullanıcı İşlemleri:**
    *   `Register`: Yeni kullanıcı kaydı (Varsayılan olarak "User" rolü atanır).
    *   `Login`: Kullanıcı girişi ve JWT Token üretimi.
*   **Rol Yönetimi (Admin Only):**
    *   `AssignRole`: Kullanıcılara rol atama (Transaction destekli, güvenli işlem).
    *   `GetUserRoles`: Bir kullanıcının rollerini listeleme.
    *   `GetUsersInRole`: Belirli bir role sahip kullanıcıları listeleme (Örn: "Staff" olanları getir).
*   **Seed Data:**
    *   Sistem ayağa kalkarken varsayılan roller (`Admin`, `Manager`, `Staff`, `User`) ve Admin kullanıcısı otomatik oluşturulur.

### 🎫 TalepService Servisi
*   **CRUD İşlemleri:**
    *   Ticket ve Attachment için tam CRUD desteği.
    *   **Repository Pattern:** `IRepository<T>`, `ITicketRepository`, `IAttachmentRepository`.
*   **İş Mantığı ve Güvenlik:**
    *   **Transaction Yönetimi:** Ticket oluşturulurken hata olursa işlemler geri alınır (Rollback).
    *   **State Transition Guard:** Ticket durum geçişleri kontrol altındadır (Örn: Closed -> New yapılamaz).
    *   **Soft Delete:** Veriler fiziksel olarak silinmez, `IsDeleted` bayrağı işaretlenir (`ISoftDeletable`).
*   **Validasyon:**
    *   `FluentValidation` ile giriş verileri kontrol edilir (Örn: Başlık boş olamaz, Öncelik belirtilmeli).
*   **İş Yükü Analizi:**
    *   `GetWorkloadStats`: Personellerin üzerindeki aktif ticket sayısını raporlar (Akıllı atama için).

### 🌐 API Gateway (Ocelot)
*   `/auth/*` -> TalepAuthentication servisine yönlendirilir.
*   `/api/ticket/*` -> TalepService servisine yönlendirilir.
*   Yetkilendirme gerektiren endpointler için JWT kontrolü aktiftir.

---

## 📂 5. Proje Dizin Yapısı

```
D:\Projects\Talep360\
├── Services\
│   ├── OcelotGateway\          # API Gateway Katmanı
│   │   └── ocelot.json         # Yönlendirme kuralları
│   ├── TalepAuthentication\    # Kimlik Yönetim Servisi
│   │   ├── Controllers\        # Auth ve Role endpointleri
│   │   ├── Services\           # İş mantığı (AuthService, RoleService)
│   │   ├── Entities\           # Veritabanı tabloları (User, Role)
│   │   └── DTOs\               # Veri transfer objeleri
│   └── TalepService\           # Talep Yönetim Servisi
│       ├── Controllers\        # Ticket endpointleri
│       ├── Services\           # TicketService, AttachmentService
│       ├── Repositories\       # Veri erişim katmanı
│       ├── Entities\           # Ticket, Attachment modelleri
│       ├── Validators\         # FluentValidation kuralları
│       └── Wrappers\           # BaseResponse standart cevap yapısı
```

---

## 🚧 6. Mevcut Durum ve Kararlar (V1)

1.  **Backend Orkestrasyonu:**
    *   Şu an için servisler (Auth ve Ticket) birbirine doğrudan HTTP isteği atmaz (Decoupled).
    *   Orkestrasyon UI (veya Postman) tarafından yönetilir.
    *   *Örnek:* Admin panelinde "Atama Yap" ekranı açıldığında; UI önce Auth servisinden "Staff" listesini çeker, sonra Ticket servisinden bu kişilerin "İş Yükünü" çeker ve ekranda birleştirir.

2.  **Hata Yönetimi:**
    *   Tüm servislerde `BaseResponse<T>` yapısı kullanılır.
    *   Global Exception Middleware ile hatalar merkezi olarak yakalanır ve standart formatta dönülür.

3.  **Güvenlik:**
    *   Hassas işlemler (Rol atama, İstatistik görme) sadece `Admin` rolüne açıktır.
    *   JWT Token olmadan API'lere erişilemez (Gateway seviyesinde engellenir).

---

## 🔜 7. Sıradaki Adımlar (Todo)
*   [ ] TalepService tarafında JWT Authentication eksikliği giderilecek (`No authenticationScheme` hatası).
*   [ ] Ticket atama (Assign) işlemi sırasında kullanıcı doğrulama mekanizması entegre edilecek.
*   [ ] Docker Compose ile tüm ortamın tek komutla ayağa kaldırılması test edilecek.

Bu doküman, projenin canlı bir özetidir ve geliştirmeler yapıldıkça güncellenmelidir.
