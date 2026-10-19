using RaceDay.Data.Models;
using System.Reflection.Emit;
using Microsoft.EntityFrameworkCore;
using RaceDay.Data.Models;

namespace RaceDay.Data;

    public sealed class ApplicationDbContext(
        DbContextOptions<ApplicationDbContext> options)
        : DbContext(options)
    {
        public DbSet<User> Users => Set<User>();
        public DbSet<RaceEvent> Events => Set<RaceEvent>();
        public DbSet<Category> Categories => Set<Category>();
        public DbSet<Enrolment> Enrolments => Set<Enrolment>();
        public DbSet<RaceResult> Results => Set<RaceResult>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<User>(entity =>
            {
                entity.HasKey(x => x.UserId);

                entity.Property(x => x.FullName)
                    .HasMaxLength(120)
                    .IsRequired();

                entity.Property(x => x.Email)
                    .HasMaxLength(256)
                    .IsRequired();

                entity.HasIndex(x => x.Email)
                    .IsUnique();

                entity.Property(x => x.PasswordHash)
                    .IsRequired();

                entity.Property(x => x.Role)
                    .HasMaxLength(32)
                    .IsRequired();
            });

            modelBuilder.Entity<RaceEvent>(entity =>
            {
                entity.HasKey(x => x.EventId);

                entity.Property(x => x.Name)
                    .HasMaxLength(150)
                    .IsRequired();

                entity.Property(x => x.Description)
                    .HasMaxLength(2000);

                entity.Property(x => x.Location)
                    .HasMaxLength(250)
                    .IsRequired();

                entity.Property(x => x.DistanceKm)
                    .HasPrecision(8, 2);

                entity.Property(x => x.EventType)
                    .HasMaxLength(16)
                    .IsRequired();

                entity.HasOne(x => x.Organiser)
                    .WithMany(x => x.Events)
                    .HasForeignKey(x => x.OrganiserId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<Category>(entity =>
            {
                entity.HasKey(x => x.CategoryId);

                entity.Property(x => x.Name)
                    .HasMaxLength(100)
                    .IsRequired();

                entity.Property(x => x.CategoryType)
                    .HasMaxLength(16)
                    .IsRequired();

                entity.Property(x => x.DistanceKm)
                    .HasPrecision(8, 2);

                entity.HasOne(x => x.Event)
                    .WithMany(x => x.Categories)
                    .HasForeignKey(x => x.EventId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<Enrolment>(entity =>
            {
                entity.HasKey(x => x.EnrolmentId);

                entity.HasIndex(x => new
                {
                    x.ParticipantId,
                    x.EventId
                }).IsUnique();

                entity.HasOne(x => x.Participant)
                    .WithMany()
                    .HasForeignKey(x => x.ParticipantId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(x => x.Event)
                    .WithMany()
                    .HasForeignKey(x => x.EventId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(x => x.Category)
                    .WithMany(x => x.Enrolments)
                    .HasForeignKey(x => x.CategoryId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(x => x.Result)
                    .WithOne(x => x.Enrolment)
                    .HasForeignKey<RaceResult>(x => x.EnrolmentId);
            });

            modelBuilder.Entity<RaceResult>(entity =>
            {
                entity.HasKey(x => x.ResultId);

                entity.HasIndex(x => x.EnrolmentId)
                    .IsUnique();
            });
        }
    }

