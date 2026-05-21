# 📌 Talep360 Proje Dokümantasyonu (PRD)

Bu belge, **Talep360** projesinin güncel durumunu, mimari kararlarını, tamamlanan özellikleri ve teknik detaylarını içermektedir.

---

## 🚀 1. Proje Özeti ve Amacı
**Talep360**, çok kiracılı (Multi-Tenant) yapıda tasarlanmış, SaaS tabanlı modern bir **Talep Yönetim Sistemi (Ticket System)** projesidir. Şirketlerin (Tenant) kendi iç süreçlerini yönetebildiği, çalışanların talepler oluşturabildiği ve yöneticilerin bu talepleri takip edebildiği ölçeklenebilir bir platformdur.

**Temel Hedef:** Kurumsal talep süreçlerini dijitalleştirmek, iş yükünü dengelemek ve rol bazlı güvenli bir yönetim sağlamak.

---

## 🏗 2. Mimari Yapı (Microservices & Frontend)

Proje, **.NET 8** tabanlı mikroservis mimarisi ve **Vue 3** tabanlı modern bir arayüzden oluşmaktadır.

### 🧩 Servisler (Backend)
1.  **OcelotGateway (API Gateway - Port: 5063):**
    *   Tüm dış isteklerin karşılandığı tek giriş noktasıdır.
    *   Routing kuralları ile istekleri `TalepAuthentication` veya `TalepService`'e yönlendirir.
    *   Örn: `/api/Tenant/{id}` isteklerini öncelikli olarak Auth servisine yönlendirir.

2.  **TalepAuthentication (Identity Service - Port: 5193):**
    *   **Identity:** Kullanıcı Kayıt, Giriş, JWT Token üretimi.
    *   **Tenant Yönetimi:** Şirket oluşturma, pasife alma (Pasif şirket kullanıcıları giriş yapamaz).
    *   **Kullanıcı Yönetimi:** Çoklu rol desteği (Multi-Role), Ünvan (Title) alanı, Personel listeleme.
    *   **Roller:** `SuperAdmin`, `Admin` (Tenant Admin), `Manager`, `Staff`, `User`.

3.  **TalepService (Ticket Service - Port: 5076):**
    *   Talep (Ticket) CRUD işlemleri.
    *   Dosya eki yönetimi.
    *   **İş Yükü Analizi:** Personellerin aktif ticket sayılarını hesaplar.
    *   State Machine mantığı ile talep durum yönetimi.

### 🖥 Arayüz (Frontend)
*   **Talep360UI:** Vue 3, TypeScript, Vite ve Tailwind CSS ile geliştirilmiş Single Page Application (SPA).
*   **Rol Bazlı Dashboardlar:**
    *   **SuperAdmin:** Tüm şirketleri, şirket personellerini, ünvanlarını ve iş yüklerini detaylı izler.
    *   **Manager:** Kendi ekibinin taleplerini ve performansını yönetir.
    *   **Staff:** Kendisine atanan talepleri görüntüler ve işlem yapar.
    *   **User:** Yeni talep oluşturur ve durumunu takip eder.

---

## 🛠 3. Teknoloji Yığını (Tech Stack)

### Backend
*   **Framework:** .NET 8 (Core)
*   **Veritabanı:** MS SQL Server (Entity Framework Core - Code First)
*   **Gateway:** Ocelot
*   **Auth:** JWT + ASP.NET Core Identity
*   **Validasyon:** FluentValidation
*   **Docs:** Swagger / OpenAPI

### Frontend
*   **Framework:** Vue 3 (Composition API)
*   **Dil:** TypeScript
*   **State Management:** Pinia
*   **Styling:** Tailwind CSS
*   **Build Tool:** Vite
*   **Router:** Vue Router (Role-based Navigation Guards)

---

## ✅ 4. Tamamlanan Özellikler ve Geliştirmeler

### 🔐 Kimlik ve Yetkilendirme (Identity & Auth)
*   **Çoklu Rol (Multi-Role) Desteği:** Bir kullanıcıya birden fazla rol atanabilir (Örn: Hem `Manager` hem `Staff`).
*   **Ünvan (Title) Sistemi:** Kullanıcılara "Kıdemli Yazılımcı", "İK Uzmanı" gibi ünvanlar eklenebilir.
*   **Tenant Güvenliği:** SuperAdmin bir şirketi pasife aldığında, o şirketin hiçbir kullanıcısı sisteme giriş yapamaz.
*   **Gelişmiş Validasyon:** Backend tarafında rol ve tenant kontrolleri sıkılaştırıldı.

### � SuperAdmin Paneli
*   **Şirket Yönetimi:** Şirketlerin listelenmesi, detaylarının görüntülenmesi.
*   **Personel İzleme:**
    *   Özel `TenantUsersView` sayfası ile seçilen şirketin tüm personelleri listelenir.
    *   Personellerin **Rolleri**, **Ünvanları** ve **Aktif İş Yükleri** (TalepService'den çekilir) tek ekranda gösterilir.
    *   İş yükü ilerleme çubukları (Progress Bars) ile görselleştirilmiştir.

### � Talep Yönetimi (Ticket)
*   **İş Yükü Dağılımı:** Talep atanırken personelin mevcut iş yoğunluğu görülür.
*   **Durum Yönetimi:** Ticket durumları kontrollü bir şekilde değiştirilir.

---

## 📂 5. Proje Dizin Yapısı

```
D:\Projects\Talep360\
├── Services\
│   ├── OcelotGateway\          # API Gateway (ocelot.json)
│   ├── TalepAuthentication\    # Identity & Tenant Servisi
│   └── TalepService\           # Ticket Yönetim Servisi
├── UI\
│   └── Talep360UI\             # Vue 3 Frontend Projesi
│       ├── src\
│       │   ├── views\          # Sayfalar (Admin, Dashboard, Ticket vb.)
│       │   ├── services\       # API istekleri (Axios)
│       │   ├── stores\         # Pinia State
│       │   └── components\     # Reusable Bileşenler
└── prd.md                      # Proje Dokümantasyonu
```

---

## ✅ 6. Mevcut Durum (v2.1)
Sistem şu anda **Multi-Tenant** yapıda, **Mikroservis** mimarisiyle ve **Modern UI** ile çalışır durumdadır.
*   ✅ Mikroservisler arası iletişim ve yönlendirme (Ocelot) sorunsuz çalışmaktadır.
*   ✅ SuperAdmin, şirketleri ve personelleri merkezi olarak yönetebilmektedir.
*   ✅ Kullanıcılar birden fazla role sahip olabilmekte ve bu roller arayüzde doğru şekilde sergilenmektedir.
*   ✅ Şirket pasife alma özelliği güvenlik katmanına entegre edilmiştir.
*   ✅ **Gelişmiş İş Akışı:** Talep atama, personel onayı ve kullanıcı yanıt sistemi (Reply Loop) devreye alınmıştır.
*   ✅ **Veri Bütünlüğü:** Çözülmüş taleplerin değiştirilmesi engellenerek (Read-Only Mode) veri güvenliği sağlanmıştır.
