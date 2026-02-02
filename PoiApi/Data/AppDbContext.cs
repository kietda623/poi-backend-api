using Microsoft.EntityFrameworkCore;
using PoiApi.Models;

namespace PoiApi.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options) { }

        // ===== USER & ROLE =====
        public DbSet<User> Users => Set<User>();
        public DbSet<Role> Roles => Set<Role>();

        // ===== POI =====
        public DbSet<POI> POIs => Set<POI>();
        public DbSet<POITranslation> POITranslations => Set<POITranslation>();

        // ===== MENU =====
        public DbSet<Menu> Menus => Set<Menu>();
        public DbSet<MenuItem> MenuItems => Set<MenuItem>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // USER - ROLE (1-N)
            modelBuilder.Entity<User>()
                .HasOne(u => u.Role)
                .WithMany(r => r.Users)
                .HasForeignKey(u => u.RoleId);

            // POI - TRANSLATION (1-N)
            modelBuilder.Entity<POITranslation>()
                .HasIndex(x => new { x.POIId, x.LanguageCode })
                .IsUnique();

            modelBuilder.Entity<POITranslation>()
                .HasOne(pt => pt.POI)
                .WithMany(p => p.Translations)
                .HasForeignKey(pt => pt.POIId);

            // POI - MENU (1-N)
            modelBuilder.Entity<Menu>()
                .HasOne(m => m.Poi)
                .WithMany(p => p.Menus)
                .HasForeignKey(m => m.PoiId);

            // MENU - MENUITEM (1-N)
            modelBuilder.Entity<MenuItem>()
                .HasOne(mi => mi.Menu)
                .WithMany(m => m.MenuItems)
                .HasForeignKey(mi => mi.MenuId);
        }
    }
}
