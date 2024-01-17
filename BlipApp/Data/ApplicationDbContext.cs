using BlipApp.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace BlipApp.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<ApplicationUser> ApplicationUsers { get; set; }
        public DbSet<Post> Posts { get; set; }
        public DbSet<Comment> Comments { get; set; }
        public DbSet<Group> Groups { get; set; }
        public DbSet<Message> Messages { get; set; }
        //public DbSet<Follower> Followers { get; set; }
        public DbSet<UserInGroup> UserInGroups { get; set; }
        public DbSet<UserFollower> UserFollowers { get; set; }


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            // definire primary key compus
            modelBuilder.Entity<UserInGroup>()
            .HasKey(ab => new
            {
                ab.Id, 
                ab.UserId,
                ab.GroupId
            });
            modelBuilder.Entity<UserInGroup>()
            .HasOne(ab => ab.User)
            .WithMany(ab => ab.UserInGroup)
            .HasForeignKey(ab => ab.UserId);

            modelBuilder.Entity<UserInGroup>()
            .HasOne(ab => ab.Group)
            .WithMany(ab => ab.UserInGroup)
            .HasForeignKey(ab => ab.GroupId);

            
        }
    }

}