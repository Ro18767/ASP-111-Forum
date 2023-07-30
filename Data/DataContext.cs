using ASP_111.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace ASP_111.Data
{
    public class DataContext : DbContext
    {
        public DbSet<User> Users { get; set; }
        public DbSet<Section> Sections { get; set; }
        public DbSet<Topic> Topics { get; set; }
        public DbSet<Theme> Themes { get; set; }
        public DbSet<Comment> Comments { get; set; }
        public DbSet<Visit> Visits { get; set; }
        public DbSet<Rate> Rates { get; set; }


        public DataContext(DbContextOptions options) : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.HasDefaultSchema("asp111forum");

            modelBuilder.Entity<Rate>()     // Указываем что в таблице Rate
                .HasKey(                    // используется композитный
                    nameof(Rate.ItemId),    // ключ по двум полям
                    nameof(Rate.UserId));   // 

            // настройка связей и навигационных свойств
            modelBuilder.Entity<Section>()
                .Ignore(e => e.Author)
                .HasOne(s => s.Author)
                .WithMany()
                .HasForeignKey(s => s.AuthorId)
                ;

            modelBuilder.Entity<Section>()
                .Ignore(s => s.Rates)
                .HasMany(s => s.Rates)
                .WithOne()
                .HasForeignKey(r => r.ItemId)
                ;

            modelBuilder.Entity<Topic>()
                .Ignore(t => t.Author)
                .HasOne(t => t.Author)
                .WithMany()
                .HasForeignKey(t => t.AuthorId)
                ;

            modelBuilder.Entity<Theme>()
                .Ignore(t => t.Author)
                .HasOne(t => t.Author)
                .WithMany()
                .HasForeignKey(t => t.AuthorId)
                ;

            modelBuilder.Entity<Theme>()
                .Ignore(t => t.Comments)
                .HasMany(t => t.Comments)
                .WithOne(c => c.Theme)
                .HasForeignKey(c => c.ThemeId)
                ;

            modelBuilder.Entity<Comment>()
                .Ignore(e => e.Author)
                .HasOne(e => e.Author)
                .WithMany()
                .HasForeignKey(e => e.AuthorId)
                ;


        }
    }
}