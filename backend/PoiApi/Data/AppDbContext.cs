using Microsoft.EntityFrameworkCore;
using PoiApi.Models;

namespace PoiApi.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options) { }

        public DbSet<User> Users => Set<User>();
        public DbSet<Role> Roles => Set<Role>();
        public DbSet<POI> POIs => Set<POI>();
        public DbSet<POITranslation> POITranslations => Set<POITranslation>();
        public DbSet<Shop> Shops { get; set; }
        public DbSet<Review> Reviews => Set<Review>();
        public DbSet<Menu> Menus => Set<Menu>();
        public DbSet<MenuItem> MenuItems => Set<MenuItem>();
        public DbSet<ServicePackage> ServicePackages => Set<ServicePackage>();
        public DbSet<Subscription> Subscriptions => Set<Subscription>();
        public DbSet<Category> Categories => Set<Category>();
        public DbSet<Language> Languages => Set<Language>();
        public DbSet<UsageHistory> UsageHistories => Set<UsageHistory>();
        public DbSet<Order> Orders => Set<Order>();
        public DbSet<SwipedItem> SwipedItems => Set<SwipedItem>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<User>()
                .HasOne(u => u.Role)
                .WithMany(r => r.Users)
                .HasForeignKey(u => u.RoleId);

            modelBuilder.Entity<POITranslation>()
                .HasIndex(x => new { x.POIId, x.LanguageCode })
                .IsUnique();

            modelBuilder.Entity<POITranslation>()
                .HasOne(pt => pt.POI)
                .WithMany(p => p.Translations)
                .HasForeignKey(pt => pt.POIId);

            modelBuilder.Entity<Menu>()
                .HasOne(m => m.Shop)
                .WithMany(p => p.Menus)
                .HasForeignKey(m => m.ShopId);

            modelBuilder.Entity<MenuItem>()
                .HasOne(mi => mi.Menu)
                .WithMany(m => m.MenuItems)
                .HasForeignKey(mi => mi.MenuId);

            modelBuilder.Entity<User>()
                .HasMany(u => u.Shops)
                .WithOne(s => s.Owner)
                .HasForeignKey(s => s.OwnerId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Shop>()
                .HasOne(s => s.Category)
                .WithMany()
                .HasForeignKey(s => s.CategoryId)
                .OnDelete(DeleteBehavior.SetNull);

            // Subscription.UserId nullable: Guest không cần tài khoản
            modelBuilder.Entity<Subscription>()
                .HasOne(s => s.User)
                .WithMany()
                .HasForeignKey(s => s.UserId)
                .IsRequired(false)
                .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<Subscription>()
                .HasOne(s => s.ServicePackage)
                .WithMany(p => p.Subscriptions)
                .HasForeignKey(s => s.ServicePackageId)
                .OnDelete(DeleteBehavior.Cascade);

            // Index cho DeviceId để tìm subscription của Guest nhanh
            modelBuilder.Entity<Subscription>()
                .HasIndex(s => s.DeviceId)
                .HasDatabaseName("IX_Subscriptions_DeviceId");

            modelBuilder.Entity<ServicePackage>().Property(p => p.MonthlyPrice).HasColumnType("decimal(18,2)");
            modelBuilder.Entity<ServicePackage>().Property(p => p.YearlyPrice).HasColumnType("decimal(18,2)");
            modelBuilder.Entity<Subscription>().Property(s => s.Price).HasColumnType("decimal(18,2)");

            modelBuilder.Entity<UsageHistory>()
                .HasOne(uh => uh.Shop)
                .WithMany()
                .HasForeignKey(uh => uh.ShopId)
                .OnDelete(DeleteBehavior.Cascade);

            // Index cho GuestId để thống kê Guest usage
            modelBuilder.Entity<UsageHistory>()
                .HasIndex(uh => uh.GuestId)
                .HasDatabaseName("IX_UsageHistories_GuestId");

            modelBuilder.Entity<Language>()
                .HasIndex(l => l.Code)
                .IsUnique();

            modelBuilder.Entity<Order>()
                .HasOne(o => o.Shop)
                .WithMany()
                .HasForeignKey(o => o.ShopId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Category>()
                .HasIndex(c => c.Slug)
                .IsUnique();

            // SwipedItem: unique constraint per user+shop, cascade delete
            modelBuilder.Entity<SwipedItem>()
                .HasIndex(si => new { si.UserId, si.ShopId })
                .IsUnique();

            modelBuilder.Entity<SwipedItem>()
                .HasOne(si => si.User)
                .WithMany(u => u.SwipedItems)
                .HasForeignKey(si => si.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<SwipedItem>()
                .HasOne(si => si.Shop)
                .WithMany()
                .HasForeignKey(si => si.ShopId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Role>().HasData(
                new Role { Id = -1, Name = RoleConstants.Admin },
                new Role { Id = -2, Name = RoleConstants.Owner },
                new Role { Id = -3, Name = RoleConstants.User }
            );

            modelBuilder.Entity<ServicePackage>().HasData(
                new ServicePackage
                {
                    Id = 1,
                    Name = "Basic",
                    Tier = "Basic",
                    Audience = RoleConstants.Owner,
                    MonthlyPrice = 99000,
                    YearlyPrice = 990000,
                    Description = "Gói khởi đầu cho shop kinh doanh nhỏ.",
                    Features = "Hiển thị trên bản đồ|Tối đa 1 gian hàng|1 Menu, không giới hạn món|Hỗ trợ qua email|!Badge trên app|!Ưu tiên đề xuất|!Thống kê nâng cao",
                    MaxStores = 1,
                    AllowAudioAccess = false,
                    AllowTinderAccess = false,
                    AllowAiPlanAccess = false,
                    AllowChatbotAccess = false,
                    IsActive = true,
                    CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc)
                },
                new ServicePackage
                {
                    Id = 2,
                    Name = "Premium",
                    Tier = "Premium",
                    Audience = RoleConstants.Owner,
                    MonthlyPrice = 299000,
                    YearlyPrice = 2990000,
                    Description = "Dành cho shop muốn tăng độ nhận diện và hiệu quả.",
                    Features = "Tất cả tính năng Basic|Tối đa 3 gian hàng|Badge \"Premium\" trên app|Ưu tiên đề xuất (score +50)|Thống kê nâng cao|Hỗ trợ ưu tiên|!Quảng cáo banner",
                    MaxStores = 3,
                    AllowAudioAccess = false,
                    AllowTinderAccess = false,
                    AllowAiPlanAccess = false,
                    AllowChatbotAccess = false,
                    IsActive = true,
                    CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc)
                },
                new ServicePackage
                {
                    Id = 3,
                    Name = "VIP",
                    Tier = "VIP",
                    Audience = RoleConstants.Owner,
                    MonthlyPrice = 599000,
                    YearlyPrice = 5990000,
                    Description = "Đặc quyền cao cấp nhất cho doanh nghiệp lớn.",
                    Features = "Tất cả tính năng Premium|Tối đa 5 gian hàng|Badge \"VIP\" trên app|Top đề xuất (score +100)|Thống kê chi tiết|Quảng cáo trên banner|Hỗ trợ riêng 24/7",
                    MaxStores = 5,
                    AllowAudioAccess = false,
                    AllowTinderAccess = false,
                    AllowAiPlanAccess = false,
                    AllowChatbotAccess = false,
                    IsActive = true,
                    CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc)
                },
                new ServicePackage
                {
                    Id = 4,
                    Name = "Audio Starter",
                    Tier = "AudioBasic",
                    Audience = RoleConstants.User,
                    MonthlyPrice = 49000,
                    YearlyPrice = 490000,
                    Description = "Gói nghe thuyết minh cơ bản.",
                    Features = "Nghe thuyết minh 3 ngôn ngữ|Hỗ trợ review sau khi nghe",
                    MaxStores = 0,
                    AllowAudioAccess = true,
                    AllowTinderAccess = false,
                    AllowAiPlanAccess = false,
                    AllowChatbotAccess = false,
                    IsActive = false,
                    CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc)
                },
                new ServicePackage
                {
                    Id = 5,
                    Name = "Audio Plus",
                    Tier = "AudioPremium",
                    Audience = RoleConstants.User,
                    MonthlyPrice = 99000,
                    YearlyPrice = 990000,
                    Description = "Gói nghe thuyết minh mở rộng.",
                    Features = "Nghe thuyết minh 3 ngôn ngữ|Ưu tiên audio mới",
                    MaxStores = 0,
                    AllowAudioAccess = true,
                    AllowTinderAccess = false,
                    AllowAiPlanAccess = false,
                    AllowChatbotAccess = false,
                    IsActive = false,
                    CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc)
                },
                new ServicePackage
                {
                    Id = 6,
                    Name = "Audio Premium",
                    Tier = "AudioVIP",
                    Audience = RoleConstants.User,
                    MonthlyPrice = 199000,
                    YearlyPrice = 1990000,
                    Description = "Gói audio cao cấp.",
                    Features = "Nghe thuyết minh 3 ngôn ngữ|Hỗ trợ ưu tiên",
                    MaxStores = 0,
                    AllowAudioAccess = true,
                    AllowTinderAccess = false,
                    AllowAiPlanAccess = false,
                    AllowChatbotAccess = false,
                    IsActive = false,
                    CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc)
                },
                // === NEW USER PACKAGES: Tour Basic & Tour Plus (Daily billing) ===
                new ServicePackage
                {
                    Id = 7,
                    Name = "Tour Basic",
                    Tier = "TourBasic",
                    Audience = RoleConstants.User,
                    MonthlyPrice = 50000,    // 50K/ngay
                    YearlyPrice = 50000,
                    Description = "Mở khóa thuyết minh ẩm thực tự động khi đến gần các gian hàng. Sử dụng trong 1 ngày.",
                    Features = "Sử dụng trong 1 ngày|Tự động phát thuyết minh khi đến gần POI|Nghe thuyết minh 3 ngôn ngữ|Hỗ trợ review sau khi nghe|!Tinder Ẩm Thực|!AI Kế Hoạch Tour|!Chatbot Thổ Địa",
                    MaxStores = 0,
                    AllowAudioAccess = true,
                    AllowTinderAccess = false,
                    AllowAiPlanAccess = false,
                    AllowChatbotAccess = false,
                    IsActive = true,
                    CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc)
                },
                new ServicePackage
                {
                    Id = 8,
                    Name = "Tour Plus",
                    Tier = "TourPlus",
                    Audience = RoleConstants.User,
                    MonthlyPrice = 99000,    // 99K/ngay
                    YearlyPrice = 99000,
                    Description = "Trải nghiệm đầy đủ: thuyết minh + Tinder ẩm thực + AI lịch trình + Chatbot tư vấn. Sử dụng trong 1 ngày.",
                    Features = "Sử dụng trong 1 ngày|Tất cả quyền lợi Tour Basic|Tinder Ẩm Thực (quẹt trái/phải)|AI Kế Hoạch Tour từ Groq|Chatbot Thổ Địa tư vấn món ăn|Ưu tiên đề xuất quán hot",
                    MaxStores = 0,
                    AllowAudioAccess = true,
                    AllowTinderAccess = true,
                    AllowAiPlanAccess = true,
                    AllowChatbotAccess = true,
                    IsActive = true,
                    CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc)
                }
            );
        }
    }
}
