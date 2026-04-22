# Tài liệu triển khai theo codebase FoodStreet

Tài liệu này triển khai các mục 1.3 -> 9.3 dựa trên hiện trạng code tại:
- Backend: `backend/PoiApi`
- Mobile App: `front-end/App-user/AppUser/AppUser`
- Web Portal: `front-end/foodstreet-admin/foodstreet-admin`

---

## 1.3 Kiến trúc tổng thể (Backend API, Mobile App, Web Portal)

Hệ thống hiện tại theo kiến trúc 3 lớp ứng dụng:

1) Backend API (ASP.NET Core .NET 8)
- Expose REST APIs cho Admin, Owner, Mobile App và Payments webhook.
- JWT authentication + role-based authorization.
- Persistence qua EF Core + MySQL.
- Tích hợp dịch vụ ngoài: Azure Speech (TTS), Azure Translator, Groq AI, PayOS.
- Realtime hub `UserTrackerHub` (SignalR).

2) Mobile App ( .NET MAUI - AppUser )
- Mô hình MVVM (CommunityToolkit.Mvvm).
- Guest-first flow: vào app với guest token, nâng cấp lên user khi đăng nhập/đăng ký.
- Các luồng chính: POI list/detail, map, audio player, QR scan, subscription, AI chatbot/tour plan.

3) Web Portal (Blazor Server)
- Một codebase cho 2 không gian: Admin portal và Owner (Seller) portal.
- Auth phía portal bằng cookie; backend authorization vẫn dựa JWT + Role.
- UI dùng MudBlazor.

### Luồng tích hợp chính
- Mobile/Web gọi API backend qua HTTP.
- Backend truy cập MySQL qua `AppDbContext`.
- Các tính năng AI/TTS/Translate/Payment gọi external APIs.
- Dashboard admin và app user kết nối SignalR hub để tracking online users.

## 1.4 Công nghệ sử dụng

Backend
- .NET 8 (ASP.NET Core Web API)
- EF Core 8 + Pomelo MySQL
- JWT Bearer Auth
- AutoMapper
- Swashbuckle (Swagger)
- BCrypt.Net-Next
- Microsoft.CognitiveServices.Speech
- QRCoder

Mobile
- .NET MAUI (target net9.0-android, windows/ios conditional)
- CommunityToolkit.Mvvm
- CommunityToolkit.Maui + MediaElement
- ZXing.Net.Maui.Controls (QR scanner)
- SignalR client

Web
- ASP.NET Core Blazor Server (.NET 8)
- MudBlazor
- Cookie Authentication + Authorize attributes
- SignalR client

External services
- Azure Speech, Azure Translator
- Groq Chat Completions API
- PayOS

---

## 2. Backend API (.NET 8)

### 2.1 Cấu trúc dự án và khởi tạo hệ thống

Cấu trúc chính:
- `Controllers/` theo domain: `Admin`, `App`, `Owner`, `Payments`.
- `Data/AppDbContext.cs` chứa DbSet và fluent mapping.
- `Models/` entity nghiệp vụ.
- `DTOs/` request/response models.
- `Services/` tích hợp nghiệp vụ và external providers.
- `Hubs/UserTrackerHub.cs` cho realtime.
- `Migrations/` quản lý vòng đời schema.

Khởi tạo trong `Program.cs`:
- Đăng ký DbContext (MySQL), DI services, AuthN/AuthZ, CORS, controllers, SignalR, Swagger.
- `Database.MigrateAsync()` khi startup.
- Seed role mặc định + đồng bộ service packages.
- Middleware chặn user `IsActive = false`.

### 2.2 Authentication (JWT + Guest Token)

Có 2 loại token:

1) User JWT
- `POST /api/auth/login` phát hành JWT với claims: `NameIdentifier`, `Email`, `Role`.
- `GET /api/auth/me`, `PUT /api/auth/profile` yêu cầu `[Authorize]`.

2) Guest JWT
- `POST /api/auth/guest-token`.
- `GuestTokenService` phát hành token role `GUEST`, claims `guest_id`, `device_id`, thời hạn 30 ngày.
- Dùng cho guest flow trên mobile để truy cập các chức năng public/limited.

### 2.3 Authorization & RBAC (ADMIN / OWNER / USER)

Role constants:
- ADMIN
- OWNER
- USER

Áp dụng authorization theo controller/action:
- Admin APIs: `[Authorize(Roles = RoleConstants.Admin)]`
- Owner APIs: `[Authorize(Roles = RoleConstants.Owner)]`
- User AI/Tinder APIs: `[Authorize(Roles = RoleConstants.User)]`
- Một số App subscription/API dùng `[Authorize]` để hỗ trợ cả USER và GUEST.

Bổ sung runtime guard:
- Middleware kiểm tra `Users.IsActive` để revoke hiệu lực thực tế của token cũ.

### 2.4 Quản lý nội dung POI và dịch đa ngôn ngữ

Entities chính:
- `POI`, `POITranslation`, `Shop`.

Quản lý nội dung:
- Admin POI CRUD tại `api/admin/pois`.
- App đọc danh sách/chi tiết qua `api/app/pois` có tham số `lang`.
- Translation fallback:
  - ưu tiên translation theo `lang`.
  - fallback về `vi`.
  - nếu chưa có, gọi Azure Translator runtime.

### 2.5 Quản lý Shop / Menu / MenuItem

Admin
- Quản lý shops, pois, menus, menuitems.

Owner
- CRUD shop của owner.
- CRUD menu owner.
- Auto tạo category mới nếu owner nhập category chưa có.
- QR code cho shop (`QrCodeService`).

App
- Đọc menu theo POI qua `api/app/pois/{poiId}/menus`.

### 2.6 Gói dịch vụ và Subscription

Mô hình:
- `ServicePackage` + `Subscription`.
- Audience tách owner/user.

Nghiệp vụ:
- Danh sách gói theo audience.
- Đăng ký gói, tạo payment link PayOS.
- Sync trạng thái thanh toán.
- Cancel subscription.
- Guest subscription hỗ trợ theo `DeviceId`.

Access check:
- `SubscriptionAccessService` kiểm tra quyền audio/tinder/ai/chatbot.

### 2.7 AI Features (AI Advisor, Tinder gợi ý)

AI endpoints (`api/ai`):
- `POST /tour-plan`
- `POST /chatbot`
- `GET /subscription-info`

Nguồn dữ liệu và rule:
- Lấy context từ shops, menus, rating, listen counts.
- Gate theo subscription feature flags.
- Có fallback deterministic khi Groq unavailable hoặc trả nội dung không hợp lệ.

Tinder (`api/app/tinder`):
- Lấy cards, swipe, liked list.
- Lưu lịch sử swipe trong `SwipedItem`.

### 2.8 Media, Audio, TTS, Translation

Media
- `api/media/upload-image`, `upload-images`.

TTS
- Azure Speech generate audio theo ngôn ngữ vi/en/zh.
- Lưu file vào `wwwroot/audio`.
- Xóa file cũ theo POI/lang trước khi tạo mới.

Translation
- Azure Translator service có retry/fallback key.
- Dùng cho POI name/description/menu labels khi cần.

### 2.9 Thanh toán và Webhook (PayOS)

Payment flow:
- Subscription controller tạo payment link qua `PayOsService`.
- Lưu `PaymentOrderCode`, `PaymentLinkId`, `CheckoutUrl`.
- Endpoint sync payment để cập nhật trạng thái định kỳ/chủ động.

Webhook:
- `POST /api/payments/payos/webhook`.
- Verify signature bằng checksum key trước khi apply trạng thái.

### 2.10 Realtime tracking (SignalR Online Users)

Hub:
- `UserTrackerHub` tại `/hubs/user-tracker`.

Hành vi:
- User app connect khi login.
- Admin dashboard connect observer mode (`source=admin-dashboard`).
- Broadcast `OnlineUsersUpdated` khi connect/disconnect.
- Track đa connection/user qua `ConcurrentDictionary`.

### 2.11 API versioning, response contract và error handling

Hiện trạng
- Chưa có versioning chính thức (`/api/v1/...` hoặc header version).
- Response contract chưa đồng nhất (object ẩn danh/string/DTO trộn lẫn).
- Error handling nằm rải rác theo từng controller.

Triển khai đề xuất theo codebase hiện tại
1) Thêm API versioning:
- Route prefix `api/v1/...`, giữ backward-compatible giai đoạn chuyển đổi.

2) Chuẩn hóa response envelope:
- Success: `{ success, data, meta }`
- Error: `{ success=false, code, message, details, traceId }`

3) Global exception handling middleware:
- Map exception -> status code chuẩn.
- Log structured error + correlation id.

4) Chuẩn hóa validation errors:
- Sử dụng `ProblemDetails` hoặc schema tương đương.

---

## 3. Mobile App (.NET MAUI - AppUser)

### 3.1 Kiến trúc MVVM và điều hướng

MVVM
- ViewModels trong `ViewModels/`.
- Services trong `Services/`.
- CommunityToolkit attributes (`[ObservableProperty]`, `[RelayCommand]`).

Điều hướng
- Shell-based tabs: Home, Explore, Scan QR, Tour Plan, Profile.
- Route registration cho các trang chi tiết: poiDetail, audioPlayer, subscriptionPackages, chat, qrScanner, ...

### 3.2 Đăng nhập/đăng ký, hồ sơ người dùng, guest flow

Flow hiện tại:
- App init: ưu tiên guest session (`GuestService` + SecureStorage).
- Login user: lưu JWT, set Authorization header, connect SignalR.
- Register user: `POST /api/auth/register-user`.
- Profile update: `PUT /api/auth/profile`.
- Logout: clear user token + disconnect SignalR.

### 3.3 Danh sách/chi tiết POI và hiển thị bản đồ

POI
- `POIService` gọi `api/app/pois` và map DTO -> model app.
- Hỗ trợ `lang` param.

Map
- `POIListPage` dùng `WebView` load `Resources/Raw/MapWebView.html`.
- Map engine: Leaflet + heatmap plugin (assets CDN).
- Cập nhật marker/user location qua JS bridge (`EvaluateJavaScriptAsync`).

GPS gần POI (geofencing nhẹ)
- Timer 10 giây check vị trí.
- Rule hiện tại: thông báo khi vào bán kính <= 100m từ POI chưa notify.

### 3.4 Audio player và xử lý audio fallback

Luồng audio:
- Lấy URL audio từ API theo ngôn ngữ.
- Download file về `FileSystem.CacheDirectory` rồi phát local bằng `MediaElement`.
- Nếu MediaElement lỗi hoặc không start kịp -> fallback WebView audio HTML.
- Track listen khi state chuyển Playing.

### 3.5 QR scanner và luồng subscription phía app

QR scanner
- Dùng ZXing.Net.Maui.Controls.
- Tab giữa mở trang scanner riêng.

Subscription
- API: `api/app/subscriptions/*`.
- Hỗ trợ package list, my subscription, history, create checkout, sync-payment, cancel.
- Với guest: dùng guest token, backend ràng theo DeviceId.

### 3.6 AI Chat và Tour Plan

- `AiService` gọi:
  - `api/ai/subscription-info`
  - `api/ai/tour-plan`
  - `api/ai/chatbot`
  - `api/app/tinder/*`
- UI gating theo quyền gói dịch vụ.

### 3.7 Localization UI và đa ngôn ngữ trên mobile

Hiện trạng:
- Có cơ chế đổi ngôn ngữ nhanh trong ViewModel (`vi/en/zh`).
- Tab title đổi theo language (`AppShell.UpdateTabTitles`).
- Một số trang đã nội địa hóa text bằng mapping nội bộ ViewModel.
- Dữ liệu nội dung POI lấy localization từ backend theo `lang`.

### 3.8 Caching phía thiết bị và chiến lược offline hiện có

Đang có:
- Guest token và guest id lưu SecureStorage.
- Audio cache theo file local trong CacheDirectory.
- Resource tĩnh (MapWebView.html, assets) đóng gói app.

Chưa có đầy đủ:
- Chưa có SQLite local cache cho POI/menu/subscription.
- Chưa có sync queue offline -> online.
- Chưa có invalidation/versioning cho data cache.

---

## 4. Web Portal (Blazor Server - Admin + Owner)

### 4.1 Kiến trúc portal và phân tách vai trò

- Một app Blazor Server với 2 khu vực route:
  - `/admin/*`
  - `/seller/*`
- Auth cookie tại portal; lưu `jwt_token` claim để gọi backend API.
- Sau login, finalize endpoint map role:
  - ADMIN -> admin dashboard
  - role còn lại -> owner dashboard

### 4.2 Admin Dashboard và thống kê

Admin pages hiện có:
- Dashboard
- Stats / Revenue
- Online Users realtime
- Settings

Nguồn dữ liệu chủ yếu từ backend:
- `api/admin/stats`
- `api/admin/stats/revenue`
- SignalR hub online users

### 4.3 Quản lý User / Seller / Store / POI

Admin pages:
- ManageUser
- ManageSeller
- ManageCustomer
- ManageStore
- ManagePoi

API backend tương ứng:
- `api/admin/users*`
- `api/admin/shops*`
- `api/admin/pois*`

### 4.4 Quản lý Category / Language / Service Package

Admin pages:
- ManageCategory
- ManageLanguage
- ManageServicePackage

Backend APIs:
- `api/admin/categories`
- `api/admin/languages`
- `api/admin/service-packages`

### 4.5 Usage History và theo dõi vận hành

- Page `UsageHistory`.
- API `api/admin/usage-history`.
- Theo dõi hành vi nghe audio theo device/guest/user + shop + duration.

### 4.6 Owner Portal: hồ sơ shop, menu, subscription, stats

Owner pages:
- Seller Dashboard
- MyStore
- MyMenus
- ServicePackages
- Stats
- Profile

APIs owner:
- `api/owner/shops*`
- `api/owner/menus*`
- `api/owner/subscriptions*`
- `api/stats/seller/*`

---

## 5. Dữ liệu và Persistence

### 5.1 Mô hình dữ liệu nghiệp vụ

Nhóm identity & quyền
- User, Role

Nhóm nội dung
- Shop, POI, POITranslation, Category, Language, Menu, MenuItem, Review

Nhóm gói dịch vụ
- ServicePackage, Subscription, Order

Nhóm hành vi
- UsageHistory, SwipedItem

### 5.2 Quan hệ bảng và ràng buộc chính

Quan hệ chính:
- User (1) - (n) Shops
- User (1) - (n) SwipedItems
- Role (1) - (n) Users
- Shop (1) - (n) Menus
- Menu (1) - (n) MenuItems
- POI (1) - (n) POITranslations
- Subscription (n) - (1) ServicePackage
- Subscription (n) - (0..1) User (nullable cho guest)
- Review (n) - (1) Shop

Ràng buộc/unique:
- `POITranslation(PoiId, LanguageCode)` unique
- `SwipedItem(UserId, ShopId)` unique
- `Language.Code` unique
- `Category.Slug` unique

### 5.3 Seed dữ liệu mặc định (Role, Service Package)

Seed roles:
- ADMIN
- OWNER
- USER

Seed service packages:
- Owner tiers: Basic/Premium/VIP
- User tiers: Audio*, TourBasic, TourPlus

Đồng bộ seed:
- startup bootstrap + `DefaultServicePackageCatalog.SyncAsync`.

### 5.4 Indexes và tối ưu truy vấn

Indexes đang có trong model builder:
- `IX_Subscriptions_DeviceId`
- `IX_UsageHistories_GuestId`
- `POITranslation(PoiId, LanguageCode)`
- `Language(Code)`
- `Category(Slug)`
- `SwipedItem(UserId, ShopId)`

Đề xuất bổ sung:
- `Subscriptions(UserId, Status, EndDate)`
- `Shops(OwnerId, IsActive)`
- `UsageHistories(ShopId, ListenedAt)`
- `Reviews(ShopId, CreatedAt)`

### 5.5 Migrations và vòng đời schema

Migrations đã có liên tục từ initial create tới:
- add menu/menuitem
- shop entity
- subscriptions/service packages
- audio fields
- reviews/stats
- AI + tour subscriptions
- guest access
- QR code URL

Quy trình vận hành hiện tại:
- App startup gọi `Database.MigrateAsync()`.

Khuyến nghị production:
- Tách migration execution khỏi startup app instance (CI/CD job hoặc migration step riêng).

---

## 6. API Endpoints (Target theo codebase hiện tại)

### 6.1 Nhóm Auth APIs

- `POST /api/auth/guest-token`
- `POST /api/auth/login`
- `GET /api/auth/me`
- `PUT /api/auth/profile`
- `POST /api/auth/register`
- `POST /api/auth/register-user`

### 6.2 Nhóm Admin APIs

Categories
- `GET/POST /api/admin/categories`
- `PUT/DELETE /api/admin/categories/{id}`

Languages
- `GET/POST /api/admin/languages`
- `PUT/DELETE /api/admin/languages/{id}`

POIs
- `GET/POST /api/admin/pois`
- `GET/PUT /api/admin/pois/{id}`
- `POST /api/admin/pois/{id}/shops`
- `POST /api/admin/pois/{id}/generate-audio`
- `GET /api/admin/pois/{id}/audio`

Shops
- `GET /api/admin/shops`
- `GET /api/admin/shops/{id}/qr`
- `POST /api/admin/shops/{id}/regenerate-qr`
- `DELETE /api/admin/shops/{id}`
- `DELETE /api/admin/shops/{id}/audio/{languageCode}`

Menus/MenuItems
- `GET/POST /api/admin/shops/{shopId}/menus`
- `GET/POST /api/admin/menus/{menuId}/items`

Users/Stats/Usage
- `GET /api/admin/users`
- `GET /api/admin/users/sellers`
- `GET /api/admin/users/customers`
- `GET /api/admin/users/me`
- `POST /api/admin/users/create-owner`
- `POST /api/admin/users/seed-customers`
- `DELETE /api/admin/users/{id}`
- `GET /api/admin/stats`
- `GET /api/admin/stats/revenue`
- `GET /api/admin/usage-history`

Service packages
- `GET /api/admin/service-packages`
- `GET /api/admin/service-packages/{id}`
- `POST /api/admin/service-packages`
- `PUT /api/admin/service-packages/{id}`
- `DELETE /api/admin/service-packages/{id}`
- `GET /api/admin/service-packages/subscriptions`

### 6.3 Nhóm App APIs

POI/App content
- `GET /api/app/pois`
- `GET /api/app/pois/{id}`
- `POST /api/app/pois/{id}/view`
- `POST /api/app/pois/{id}/listen`
- `POST /api/app/pois/{id}/reviews`
- `GET /api/app/pois/{poiId}/menus`
- `GET /api/app/menus/{menuId}/items`

User subscriptions
- `GET /api/app/subscriptions/packages`
- `GET /api/app/subscriptions/my`
- `GET /api/app/subscriptions/history`
- `POST /api/app/subscriptions`
- `POST /api/app/subscriptions/{id}/sync-payment`
- `DELETE /api/app/subscriptions/{id}`

AI/Tinder
- `POST /api/ai/tour-plan`
- `POST /api/ai/chatbot`
- `GET /api/ai/subscription-info`
- `GET /api/app/tinder/cards`
- `POST /api/app/tinder/swipe`
- `GET /api/app/tinder/liked`

### 6.4 Nhóm Owner APIs

Shops
- `GET /api/owner/shops`
- `POST /api/owner/shops`
- `PUT /api/owner/shops/{id}`
- `DELETE /api/owner/shops/{id}`
- `GET /api/owner/shops/{id}/qr`
- `POST /api/owner/shops/{id}/regenerate-qr`
- `POST /api/owner/shops/{id}/generate-tts`
- `POST /api/owner/shops/{id}/generate-tts-all`
- `POST /api/owner/shops/translate`

Menus
- `GET /api/owner/menus`
- `POST /api/owner/menus`
- `PUT /api/owner/menus/{id}`
- `DELETE /api/owner/menus/{id}`

Subscriptions
- `GET /api/owner/subscriptions/packages`
- `GET /api/owner/subscriptions/my`
- `GET /api/owner/subscriptions/history`
- `POST /api/owner/subscriptions`
- `POST /api/owner/subscriptions/{id}/sync-payment`
- `DELETE /api/owner/subscriptions/{id}`

Stats
- `GET /api/stats/seller/{sellerId}`
- `GET /api/stats/seller/{sellerId}/revenue`
- `GET /api/stats/seller/{sellerId}/overview`
- `GET /api/stores/{storeId}/reviews`

### 6.5 Nhóm Payments / Media APIs

- `POST /api/payments/payos/webhook`
- `POST /api/media/upload-image`
- `POST /api/media/upload-images`

### 6.6 Quy tắc bảo mật endpoint

1) Authentication
- Tất cả endpoint cần dữ liệu riêng tư phải yêu cầu bearer token.

2) Authorization
- Endpoint admin/owner/user bắt buộc role-based guard.

3) Ownership check
- Owner chỉ thao tác tài nguyên thuộc owner hiện tại (đã có ở nhiều endpoint).

4) Webhook security
- Bắt buộc verify chữ ký PayOS trước khi cập nhật payment status.

5) Input validation
- Chuẩn hóa DTO validation + giới hạn payload size cho upload.

---

## 7. Non-Functional Requirements

### 7.1 Security

Đã có
- JWT bearer + role auth.
- BCrypt password hash.
- IsActive guard middleware.
- PayOS signature verify.

Cần bổ sung
- Không commit secret thật trong appsettings.
- CORS giới hạn origin cụ thể thay vì AllowAnyOrigin.
- Rate limit cho auth, AI, payment sync, media upload.
- Audit log cho hành động quản trị.

### 7.2 Performance

Đã có
- Indexes nền tảng cho translation/swipe/subscription/usage.
- Một số truy vấn dùng projection.

Cần bổ sung
- Pagination cho list lớn (users/shops/usage history).
- Response caching cho metadata ít đổi (languages/packages/categories).
- Giảm N+1 queries ở một số endpoint include sâu.

### 7.3 Reliability

Đã có
- Fallback ở AI/TTS flows.
- Subscription state sync qua API.

Cần bổ sung
- Retry policy có backoff cho external APIs (Polly).
- Dead-letter/log queue cho webhook fail.
- Health checks cho DB và external dependencies.

### 7.4 Scalability

Đã có
- Service layer tách khỏi controllers.

Cần bổ sung
- Stateless scaling cho API instances.
- Distributed cache/session strategy (nếu mở rộng).
- Background workers cho tác vụ nặng (TTS batch, translation batch).

### 7.5 Logging và Monitoring

Hiện trạng
- Logging chủ yếu console + debug.

Đề xuất
- Structured logging (Serilog/OpenTelemetry).
- CorrelationId xuyên suốt request.
- Metrics: latency/error rate per endpoint, payment conversion, AI usage.
- Alerting cho webhook thất bại, external API timeout, DB errors.

---

## 8. Kiểm thử và triển khai

### 8.1 Test strategy (API, Mobile, Web)

Hiện trạng
- Chưa có test project dedicated trong repo.

Chiến lược triển khai
1) API tests
- Unit tests cho services (subscription, payment state, ai gating).
- Integration tests cho controllers chính.

2) Mobile tests
- ViewModel unit tests cho navigation/commands/state transitions.
- Smoke tests cho auth, poi list, audio flow.

3) Web tests
- Component tests cho pages quan trọng.
- E2E smoke cho login + admin/owner critical paths.

### 8.2 Kiểm thử tích hợp và kiểm thử nghiệp vụ

Các kịch bản ưu tiên:
- Login/register/guest-token.
- Subscription checkout -> webhook/sync -> active status.
- Audio permission gate theo package.
- Owner tạo shop/menu + generate tts.
- Admin quản lý users/shops/packages.
- AI endpoints với đủ/thiếu quyền.

### 8.3 Cấu hình môi trường (Development/Staging/Production)

Dev
- `appsettings.Development.json` cho local keys và local DB.

Staging/Prod
- Secrets qua environment variables/secret manager.
- Connection string tách theo môi trường.
- Cấu hình HTTPS, CORS whitelist, callback URLs (PayOS) theo domain thật.

### 8.4 Quy trình build, release và rollback

Build đề xuất:
- Backend: `dotnet build backend/PoiApi/PoiApi.csproj`
- Web: `dotnet build front-end/foodstreet-admin/foodstreet-admin/foodstreet-admin.csproj`
- Mobile: build theo target platform MAUI.

Release đề xuất:
1) CI build + test + migration validation.
2) Deploy staging, chạy smoke tests.
3) Deploy production (blue/green hoặc rolling).

Rollback:
- App version rollback + DB backward plan.
- Với migration breaking: cần script rollback được kiểm thử trước.

---

## 9. Hạn chế hiện tại và định hướng mở rộng

### 9.1 Các phần đã có trong codebase

- Full stack core chạy được: backend + mobile + web portal.
- RBAC cơ bản hoạt động.
- Subscription + PayOS flow có tạo link/sync/webhook.
- AI tour/chatbot đã tích hợp external model.
- Realtime online users đã có qua SignalR.
- QR scan và QR code generation đã có.

### 9.2 Các phần cần bổ sung

1) Geofencing sâu
- Hiện chỉ geofence client-side đơn giản (100m, timer 10s).
- Chưa có geofencing policy configurable từ backend.

2) Offline sync nâng cao
- Chưa có SQLite cache cho POI/menu/subscription.
- Chưa có delta sync, conflict resolution, retry queue.

3) Map pack / offline map
- Hiện map phụ thuộc tile online CDN.
- Chưa có map pack download/offline tiles.

4) Chuẩn hóa API contract/versioning
- Chưa có versioning chính thức và unified response envelope.

5) Test automation và observability
- Chưa có bộ test tự động toàn diện.
- Monitoring/logging chưa đủ cho production scale.

### 9.3 Lộ trình nâng cấp theo giai đoạn

Giai đoạn 1 (ổn định nền tảng)
- Chuẩn hóa response/error contract.
- Thêm rate limiting + CORS hardening + secret management.
- Bổ sung test API critical paths.

Giai đoạn 2 (trải nghiệm người dùng)
- SQLite offline cache cho mobile.
- Delta sync cho POI và subscription.
- Geofencing config-driven (bán kính, cooldown, language).

Giai đoạn 3 (mở rộng vận hành)
- Structured logging + metrics + alerting.
- Background jobs cho TTS/translation batch.
- Triển khai scale-out và tối ưu DB/index theo traffic thật.
