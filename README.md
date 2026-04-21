Đồ án: Hệ thống thuyết minh đa ngôn ngữ cho phố ẩm thực Vĩnh Khánh   

Thành viên:
    - Nguyễn Thùy Ánh Minh - 3123411189
    - Dương ANh Kiệt - 3123411169

Giới thiệu: Dành cho khách du lịch nước ngoài và trong nước có nhu cầu tìm hiểu các thông tin cần thiết cho từng món ăn, từng gian hàng trong phố ẩm thực. 
Khi du khách đăng ký một trong các gói dịch vụ của hệ thống, hệ thống sẽ cung cấp cho du khách các thông tin cần thiết cho từng món ăn, từng gian hàng trong 
phố ẩm thực bằng hệ thống thuyết minh tự động. Khi du khách vào phạm vi bán kính của gian hàng, thuyết minh sẽ tự động phát cho du khách nghe. 
Hệ thống kết hợp định vị GPS, trí tuệ nhân tạo (Chatbot AI) và công nghệ Text-to-Speech để tự động hóa việc hướng dẫn, giới thiệu lịch sử và 
tư vấn các món ăn đặc sắc cho du khách theo thời gian thực.

Công nghệ: .NET MAUI  |  Blazor/MudBlazor  |  ASP.NET Core  |  MySQL

Cấu trúc thư mục dự án:
foodtour/
├── backend/
│   └── PoiApi/              ← Backend API (.NET 8)
├── front-end/
│   ├── foodstreet-admin/    ← Admin Dashboard (Blazor .NET 8)
│   └── App-user/AppUser/   ← App người dùng (.NET MAUI 9)
└── POI_FoodTour.sql         ← File SQL khởi tạo database

Hướng dẫn cài đặt:

    * Yêu cầu phần mềm:
        - .NET SDK 9.0 (MAUI) + 8.0 (Backend/Admin)
        - Visual Studio 2022 17.9+ (có workload MAUI + ASP.NET)
        - MySQL Server 8.0+
        - MySQL Workbench Tùy chọn, để quản lý DB dễ hơn
        - Git Bất kỳ
    
    * Hướng dẫn từng bước để triển khai và khởi chạy toàn bộ hệ thống FoodStreet POI gồm 3 thành phần chính:
        - Backend API – ASP.NET Core 8 (REST API + Swagger)
        - Admin Dashboard – Blazor Server (quản trị hệ thống)
        - App Người Dùng – .NET MAUI (ứng dụng Android cho khách tham quan)

    * Các bước thực hiện:
        Bước 1 — Cài Đặt Cơ Sở Dữ Liệu (MySQL)
                1.1. Tạo database và import dữ liệu
                    Mở MySQL Workbench hoặc dùng MySQL Command Line Client, thực hiện lệnh sau:

                    sql
                    -- Cách 1: Dùng MySQL Command Line
                    mysql -u root -p < đường_dẫn_đến/POI_FoodTour.sql
                    Hoặc trong MySQL Workbench:

                    Vào menu File → Open SQL Script
                    Chọn file POI_FoodTour.sql trong thư mục gốc dự án
                    Nhấn nút Execute (⚡ Sấm sét)
                1.2. Xác nhận database đã được tạo
                    sql
                    SHOW DATABASES;
                    -- Kết quả phải có: poi_foodtour
                    USE poi_foodtour;
                    SHOW TABLES;
                    Kết quả mong đợi sẽ có các bảng: categories, menuitems, menus, orders, pois, poitranslations, reviews, roles, servicepackages, shops, subscriptions, usagehistories, users

                NOTE

                File SQL đã bao gồm dữ liệu mẫu. Tài khoản admin mặc định là: admin@foodstreet.vn

        Bước 2 — Cấu Hình & Khởi Chạy Backend API
                2.1. Mở dự án
                    Mở file backend/PoiApi.slnx bằng Visual Studio 2022.

                2.2. Cấu hình appsettings.json
                    Mở file backend/PoiApi/appsettings.json và điền đúng thông tin vào các trường BẮT BUỘC:

                    json
                    {
                    "ConnectionStrings": {
                        "MySql": "server=localhost;port=3306;database=POI_FoodTour;user=root;password=MẬT_KHẨU_MYSQL_CỦA_BẠN"
                    },
                    "Jwt": {
                        "Key": "THIS_IS_A_SUPER_SECRET_KEY_123456",
                        "Issuer": "PoiApi",
                        "Audience": "PoiApiClient"
                    },
                    "Azure": {
                        "SpeechKey": "KEY_AZURE_SPEECH_CỦA_BẠN",
                        "SpeechRegion": "southeastasia",
                        "TranslatorKey": "KEY_AZURE_TRANSLATOR_CỦA_BẠN",
                        "TranslatorRegion": "global",
                        "TranslatorEndpoint": "https://api.cognitive.microsofttranslator.com/"
                    },
                    "PayOS": {
                        "ClientId": "CLIENT_ID_PAYOS_CỦA_BẠN",
                        "ApiKey": "API_KEY_PAYOS_CỦA_BẠN",
                        "ChecksumKey": "CHECKSUM_KEY_PAYOS_CỦA_BẠN"
                    },
                    "Groq": {
                        "ApiKey": "API_KEY_GROQ_CỦA_BẠN"
                    }
                }
            WARNING

            Không bỏ qua phần này! Nếu thiếu API key thực, các tính năng sau sẽ không hoạt động:

            Azure Speech/Translator: Tổng hợp và dịch thuật âm thanh
            PayOS: Thanh toán gói dịch vụ
            Groq: Chatbot AI tư vấn
            Hướng dẫn lấy API Keys:

            Service	Link đăng ký
            Azure Speech + Translator	https://portal.azure.com → Tạo resource "Speech service" và "Translator"
            PayOS	https://business.payos.vn → Đăng nhập → Developer → API Keys
            Groq	https://console.groq.com → Dashboard → API Keys
            2.3. Kiểm tra kết nối và khởi chạy
            Trong Visual Studio, đặt project PoiApi làm Startup Project, rồi nhấn F5 hoặc Ctrl+F5 để chạy.

            Hoặc dùng terminal:

            powershell
            cd backend/PoiApi
            dotnet run
            ✅ Backend khởi chạy thành công khi trình duyệt tự mở:

            Swagger UI: http://localhost:5279/swagger
            NOTE

            Khi khởi chạy lần đầu, hệ thống tự động:

            Chạy Database Migration
            Seed dữ liệu Roles (ADMIN, OWNER, USER)
            Seed bảng Service Packages (Basic, Premium, VIP, các gói Audio)
        Bước 3 — Cấu Hình & Khởi Chạy Admin Dashboard
            3.1. Mở dự án
                Mở file front-end/foodstreet-admin/foodstreet-admin.slnx bằng Visual Studio 2022.

            3.2. Cấu hình URL Backend
                Mở file front-end/foodstreet-admin/foodstreet-admin/appsettings.json và đảm bảo URL trỏ đến Backend API:

                json
                {
                    "ApiBaseUrl": "http://localhost:5279/api/"
                }
            IMPORTANT

            Port 5279 phải khớp với port mà Backend API đang chạy. Kiểm tra trong Properties/launchSettings.json của PoiApi nếu cần.

            3.3. Khởi chạy
                Đặt project foodstreet-admin làm Startup Project, nhấn F5.

                Hoặc dùng terminal:

                powershell
                cd front-end/foodstreet-admin/foodstreet-admin
                dotnet run
            ✅ Admin Dashboard truy cập tại: http://localhost:5231 (hoặc port được assign)

            3.4. Đăng nhập Admin
                Thông tin	Giá trị
                Email	admin@foodstreet.vn
                Mật khẩu	Admin@1234 (hoặc mật khẩu đã set trong DB)
        Bước 4 — Build & Cài Đặt App Người Dùng (Android)
            4.1. Yêu cầu thêm cho MAUI Android
                Cài Android SDK thông qua Visual Studio Installer
                Kết nối điện thoại Android (bật Developer Options + USB Debugging)
                Hoặc cài Android Emulator từ Visual Studio
            4.2. Mở dự án
                Mở file front-end/App-user/AppUser/AppUser.slnx bằng Visual Studio 2022.

            4.3. Cấu hình URL Backend trong App
                Tìm file cấu hình API trong front-end/App-user/AppUser/AppUser/Services/ và cập nhật địa chỉ backend:

                csharp
                // Nếu chạy trên emulator
                private const string BaseUrl = "http://10.0.2.2:5279/api/";
                // Nếu chạy trên thiết bị thật (cùng mạng WiFi)
                private const string BaseUrl = "http://192.168.x.x:5279/api/";
                // Thay 192.168.x.x bằng IP thật của máy tính chạy backend
            IMPORTANT

            Lưu ý quan trọng khi kết nối thiết bị thật với Backend:

            Thiết bị Android và máy tính phải cùng mạng WiFi
            Backend API phải lắng nghe trên 0.0.0.0 (không phải chỉ localhost)
            Kiểm tra Firewall Windows — cho phép port 5279
            4.4. Build và deploy lên thiết bị Android
                Trong Visual Studio, chọn thiết bị Android mục tiêu ở thanh toolbar
                Nhấn F5 để Build và Deploy
                Hoặc build file APK để cài thủ công:

                powershell
                cd front-end/App-user/AppUser/AppUser
                dotnet publish -f net9.0-android -c Release
                File APK sẽ xuất hiện trong thư mục bin/Release/net9.0-android/publish/.
        Bước 5 — Kiểm Tra Toàn Hệ Thống
            Sau khi cài đặt xong, kiểm tra theo thứ tự sau:

            ✅ Checklist kiểm tra
                - MySQL đang chạy và database poi_foodtour tồn tại
                - Backend API chạy thành công tại http://localhost:5279/swagger
                - Truy cập /swagger → Thử GET /api/poi → Nhận về danh sách POI
                - Admin Dashboard truy cập được tại http://localhost:5231
                - Đăng nhập Admin thành công
                - Xem được danh sách Sellers và Shops
                - App Android cài được trên thiết bị / emulator
                - Đăng ký tài khoản người dùng mới trên App
                - Xem bản đồ POI trên App
