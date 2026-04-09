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

            modelBuilder.Entity<Subscription>()
                .HasOne(s => s.User)
                .WithMany()
                .HasForeignKey(s => s.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Subscription>()
                .HasOne(s => s.ServicePackage)
                .WithMany(p => p.Subscriptions)
                .HasForeignKey(s => s.ServicePackageId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<ServicePackage>().Property(p => p.MonthlyPrice).HasColumnType("decimal(18,2)");
            modelBuilder.Entity<ServicePackage>().Property(p => p.YearlyPrice).HasColumnType("decimal(18,2)");
            modelBuilder.Entity<Subscription>().Property(s => s.Price).HasColumnType("decimal(18,2)");

            modelBuilder.Entity<UsageHistory>()
                .HasOne(uh => uh.Shop)
                .WithMany()
                .HasForeignKey(uh => uh.ShopId)
                .OnDelete(DeleteBehavior.Cascade);

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
                    IsActive = true,
                    CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc)
                },
                new ServicePackage
                {
                    Id = 4,
                    Name = "Audio Starter",
                    Tier = "Basic",
                    Audience = RoleConstants.User,
                    MonthlyPrice = 49000,
                    YearlyPrice = 490000,
                    Description = "Goi nghe thuyet minh co ban danh cho nguoi dung app.",
                    Features = "Nghe thuyet minh 3 ngon ngu|Ho tro review sau khi nghe|Su dung tren toan bo app",
                    MaxStores = 0,
                    AllowAudioAccess = true,
                    IsActive = true,
                    CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc)
                },
                new ServicePackage
                {
                    Id = 5,
                    Name = "Audio Plus",
                    Tier = "Premium",
                    Audience = RoleConstants.User,
                    MonthlyPrice = 99000,
                    YearlyPrice = 990000,
                    Description = "Goi audio uu tien danh cho nguoi nghe thuong xuyen.",
                    Features = "Nghe thuyet minh 3 ngon ngu|Uu tien audio moi|Khong gioi han luot nghe trong goi con han",
                    MaxStores = 0,
                    AllowAudioAccess = true,
                    IsActive = true,
                    CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc)
                },
                new ServicePackage
                {
                    Id = 6,
                    Name = "Audio Premium",
                    Tier = "VIP",
                    Audience = RoleConstants.User,
                    MonthlyPrice = 199000,
                    YearlyPrice = 1990000,
                    Description = "Goi audio cao cap cho nguoi dung trung thanh.",
                    Features = "Nghe thuyet minh 3 ngon ngu|Truy cap tat ca diem audio|Ho tro uu tien",
                    MaxStores = 0,
                    AllowAudioAccess = true,
                    IsActive = true,
                    CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc)
                }
            );
        }
    }
}
