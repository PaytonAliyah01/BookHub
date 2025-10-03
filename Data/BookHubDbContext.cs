using Microsoft.EntityFrameworkCore;
using BookHub.Models;

namespace BookHub.Data
{
    public class BookHubDbContext : DbContext
    {
        public BookHubDbContext(DbContextOptions<BookHubDbContext> options) : base(options) { }

        public DbSet<User> Users { get; set; }
        public DbSet<Book> Books { get; set; }
        public DbSet<UserBook> UserBooks { get; set; }
        public DbSet<Review> Reviews { get; set; }
        public DbSet<ReadingGoal> ReadingGoals { get; set; }
        public DbSet<BookClub> BookClubs { get; set; }
        public DbSet<ClubMembership> ClubMemberships { get; set; }
        public DbSet<ClubDiscussion> ClubDiscussions { get; set; }
        public DbSet<Friendship> Friendships { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Primary keys
            modelBuilder.Entity<User>().HasKey(u => u.UserId);
            modelBuilder.Entity<Book>().HasKey(b => b.BookId);
            modelBuilder.Entity<UserBook>().HasKey(ub => ub.UserBookId);
            modelBuilder.Entity<Review>().HasKey(r => r.ReviewId);
            modelBuilder.Entity<ReadingGoal>().HasKey(rg => rg.ReadingGoalId);
            modelBuilder.Entity<BookClub>().HasKey(c => c.ClubId);
            modelBuilder.Entity<ClubMembership>().HasKey(cm => cm.ClubMembershipId);
            modelBuilder.Entity<ClubDiscussion>().HasKey(cd => cd.ClubDiscussionId);
            modelBuilder.Entity<Friendship>().HasKey(f => f.FriendshipId);

            // Relationships
            modelBuilder.Entity<UserBook>()
                .HasOne(ub => ub.User)
                .WithMany(u => u.UserBooks)
                .HasForeignKey(ub => ub.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<UserBook>()
                .HasOne(ub => ub.Book)
                .WithMany(b => b.UserBooks)
                .HasForeignKey(ub => ub.BookId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Review>()
                .HasOne(r => r.User)
                .WithMany(u => u.Reviews)
                .HasForeignKey(r => r.UserId)
                .OnDelete(DeleteBehavior.NoAction); // prevent multiple cascade paths

            modelBuilder.Entity<Review>()
                .HasOne(r => r.Book)
                .WithMany(b => b.Reviews)
                .HasForeignKey(r => r.BookId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<ReadingGoal>()
                .HasOne(rg => rg.User)
                .WithMany(u => u.ReadingGoals)
                .HasForeignKey(rg => rg.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<BookClub>()
                .HasOne(c => c.Owner)
                .WithMany(u => u.OwnedClubs)
                .HasForeignKey(c => c.OwnerId)
                .OnDelete(DeleteBehavior.NoAction); // prevents cascade conflicts

            modelBuilder.Entity<ClubMembership>()
                .HasOne(cm => cm.User)
                .WithMany(u => u.ClubMemberships)
                .HasForeignKey(cm => cm.UserId)
                .OnDelete(DeleteBehavior.NoAction); // fix cascade problem

            modelBuilder.Entity<ClubMembership>()
                .HasOne(cm => cm.Club)
                .WithMany(c => c.Members)
                .HasForeignKey(cm => cm.ClubId)
                .OnDelete(DeleteBehavior.Cascade); // safe to cascade here

            modelBuilder.Entity<ClubDiscussion>()
                .HasOne(cd => cd.User)
                .WithMany()
                .HasForeignKey(cd => cd.UserId)
                .OnDelete(DeleteBehavior.NoAction); // prevent multiple cascade paths

            modelBuilder.Entity<ClubDiscussion>()
                .HasOne(cd => cd.Club)
                .WithMany(c => c.Discussions)
                .HasForeignKey(cd => cd.ClubId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Friendship>()
                .HasOne(f => f.User)
                .WithMany(u => u.Friendships)
                .HasForeignKey(f => f.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Friendship>()
                .HasOne(f => f.Friend)
                .WithMany()
                .HasForeignKey(f => f.FriendId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Friendship>()
                .HasIndex(f => new { f.UserId, f.FriendId })
                .IsUnique();
        }
    }
}
