using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Hellion.Core.Configuration;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace Hellion.Core.Database
{
    public class DatabaseContext : DbContext
    {
        private readonly DatabaseConfiguration? configuration;

        public DatabaseContext(DatabaseConfiguration configuration)
            : base()
        {
            this.configuration = configuration;
        }

        public DatabaseContext(DbContextOptions<DatabaseContext> options)
            : base(options)
        {
        }

        public DbSet<DbUser> Users { get; set; } = null!;

        public DbSet<DbCharacter> Characters { get; set; } = null!;

        public DbSet<DbItem> Items { get; set; } = null!;

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (optionsBuilder.IsConfigured || this.configuration == null) return;

            var connectionString = string.Format(
                "server={0};userid={1};pwd={2};port=3306;database={3};sslmode=none;CharSet=utf8mb4;",
                this.configuration.Ip,
                this.configuration.User,
                this.configuration.Password,
                this.configuration.DatabaseName);

            optionsBuilder.UseMySql(connectionString, new MySqlServerVersion(new System.Version(8, 0, 21)));
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.HasCharSet("utf8mb4");
            modelBuilder.UseCollation("utf8mb4_unicode_ci");

            modelBuilder.Entity<DbUser>(entity =>
            {
                entity.HasIndex(u => u.Username).IsUnique().HasDatabaseName("IX_users_username");
            });

            modelBuilder.Entity<DbCharacter>(entity =>
            {
                entity.HasIndex(c => c.AccountId).HasDatabaseName("IX_characters_accountId");
                entity.HasIndex(c => c.Name).IsUnique().HasDatabaseName("IX_characters_name");
            });

            modelBuilder.Entity<DbItem>(entity =>
            {
                entity.HasOne(i => i.Character)
                    .WithMany(c => c.Items)
                    .HasForeignKey(i => i.CharacterId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasIndex(i => i.CharacterId).HasDatabaseName("IX_items_characterId");
            });

            base.OnModelCreating(modelBuilder);
        }

        public override int SaveChanges()
        {
            this.ApplyTimestamps();
            return base.SaveChanges();
        }

        public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            this.ApplyTimestamps();
            return base.SaveChangesAsync(cancellationToken);
        }

        private void ApplyTimestamps()
        {
            var now = DateTime.UtcNow;
            foreach (var entry in this.ChangeTracker.Entries<BaseEntity>())
            {
                if (entry.State == EntityState.Added)
                {
                    entry.Entity.CreatedAt = now;
                    entry.Entity.UpdatedAt = now;
                }
                else if (entry.State == EntityState.Modified)
                {
                    entry.Entity.UpdatedAt = now;
                }
            }
        }
    }
}
