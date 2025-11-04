using Microsoft.EntityFrameworkCore;
using TaskManager.Domain.Entities;

namespace TaskManager.Infrastructure.DB
{
    public class UserContext : DbContext
    {
        public DbSet<User> Users => Set<User>();
        public DbSet<Note> Notes => Set<Note>();
        public DbSet<UserConfirmation> UserConfirmations => Set<UserConfirmation>();
        public DbSet<Notification> Notifications => Set<Notification>();
        public DbSet<Chat> Chats => Set<Chat>();
        public DbSet<Message> Messages => Set<Message>();

        public UserContext(DbContextOptions<UserContext> options) : base(options)
        {
            Database.EnsureCreated();
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Настройка User
            modelBuilder.Entity<User>()
                .OwnsOne(u => u.AuthorizationParams, ap =>
                {
                    ap.Property(p => p.EMail).HasColumnName("Email");
                    ap.Property(p => p.PasswordHash).HasColumnName("PasswordHash");
                })
                .OwnsOne(u => u.UserStats, ap =>
                {
                    ap.Property(p => p.AvatarPath).HasColumnName("Avatar");
                    ap.Property(p => p.NickName).HasColumnName("NickName");
                })
                .OwnsOne(u => u.FriendList, ap =>
                {
                    ap.Property(p => p.Friends).HasColumnName("Friends");
                    ap.Property(p => p.ConsiderationAppl).HasColumnName("ConsiderationAppl");
                    ap.Property(p => p.BlockedUsers).HasColumnName("BlockedUsers");
                })
                .HasMany(u => u.Chats)
                .WithMany(c => c.Members)
                .UsingEntity(j => j.ToTable("Enrollments"));

            // Настройка Chat
            modelBuilder.Entity<Chat>(entity =>
            {
                entity.HasKey(c => c.Id);

                entity.HasMany(c => c.Messages)
                    .WithOne(m => m.Chat)
                    .HasForeignKey(m => m.ChatId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // Настройка Message
            modelBuilder.Entity<Message>(entity =>
            {
                entity.HasKey(m => m.Id);

                entity.Property(m => m.Body)
                      .IsRequired()
                      .HasMaxLength(2000);

                entity.Property(m => m.SendTime)
                      .IsRequired();

                entity.HasOne(m => m.Sender)
                      .WithMany()
                      .HasForeignKey(m => m.SenderId)
                      .OnDelete(DeleteBehavior.Restrict);
            });
        }
    }
}
