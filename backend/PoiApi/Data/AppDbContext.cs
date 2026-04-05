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

        public DbSet<Menu> Menus => Set<Menu>();
        public DbSet<MenuItem> MenuItems => Set<MenuItem>();

        public DbSet<ServicePackage> ServicePackages => Set<ServicePackage>();
        public DbSet<Subscription> Subscriptions => Set<Subscription>();

        public DbSet<Category> Categories => Set<Category>();
        public DbSet<Language> Languages => Set<Language>();
        public DbSet<UsageHistory> UsageHistories => Set<UsageHistory>();

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

            // ── ServicePackage ──────────────────────────────────────
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

            // ── UsageHistory ────────────────────────────────────────
            modelBuilder.Entity<UsageHistory>()
                .HasOne(uh => uh.Shop)
                .WithMany()
                .HasForeignKey(uh => uh.ShopId)
                .OnDelete(DeleteBehavior.Cascade);

            // ── Language unique index ───────────────────────────────
            modelBuilder.Entity<Language>()
                .HasIndex(l => l.Code)
                .IsUnique();

            // ── Category unique index ───────────────────────────────
            modelBuilder.Entity<Category>()
                .HasIndex(c => c.Slug)
                .IsUnique();

            // ── Seed data ───────────────────────────────────────────
            modelBuilder.Entity<Role>().HasData(
                new Role { Id = -1, Name = RoleConstants.Admin },
                new Role { Id = -2, Name = RoleConstants.Owner },
                new Role { Id = -3, Name = RoleConstants.User }
            );

            modelBuilder.Entity<ServicePackage>().HasData(
                new ServicePackage
                {
                    Id = 1, Name = "Gói Cơ bản", Tier = "Basic",
                    MonthlyPrice = 99_000, YearlyPrice = 990_000,
                    Description = "Gói dành cho gian hàng mới bắt đầu",
                    Features = "Hiển thị trên bản đồ|1 gian hàng|Hỗ trợ qua email",
                    MaxStores = 1, IsActive = true,
                    CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc)
                },
                new ServicePackage
                {
                    Id = 2, Name = "Gói Nâng cao", Tier = "Premium",
                    MonthlyPrice = 299_000, YearlyPrice = 2_990_000,
                    Description = "Ưu tiên đề xuất và badge Premium trên app",
                    Features = "Ưu tiên đề xuất|Badge Premium|3 gian hàng|Hỗ trợ ưu tiên|Thống kê nâng cao",
                    MaxStores = 3, IsActive = true,
                    CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc)
                },
                new ServicePackage
                {
                    Id = 3, Name = "Gói VIP", Tier = "VIP",
                    MonthlyPrice = 599_000, YearlyPrice = 5_990_000,
                    Description = "Top đề xuất, badge VIP và hỗ trợ riêng 24/7",
                    Features = "Top đề xuất trên app|Badge VIP|5 gian hàng|Hỗ trợ riêng 24/7|Thống kê chi tiết|Quảng cáo trên banner",
                    MaxStores = 5, IsActive = true,
                    CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc)
                }
            );


        }
    }
}
