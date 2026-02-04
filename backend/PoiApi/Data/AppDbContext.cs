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
                .HasOne(u => u.Shop)
                .WithOne(s => s.Owner)
                .HasForeignKey<Shop>(s => s.OwnerId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Role>().HasData(
    new Role { Id = -1, Name = RoleConstants.Admin },
    new Role { Id = -2, Name = RoleConstants.Owner },
    new Role { Id = -3, Name = RoleConstants.User }
);


        }
    }
}
