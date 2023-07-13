using ISocialBlog.Data.Mappings;
using ISocialBlog.Models;
using Microsoft.EntityFrameworkCore;

namespace ISocialBlog.Data
{
    public class ISocialBlogDataContext : DbContext
    {

        public ISocialBlogDataContext(DbContextOptions<ISocialBlogDataContext> options) : base(options)
        {
            
        }

        public DbSet<Category> Categories { get; set; }
        public DbSet<Post> Posts { get; set; }
        public DbSet<User> Users { get; set; }


       
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfiguration(new CategoryMap());
            modelBuilder.ApplyConfiguration(new UserMap());
            modelBuilder.ApplyConfiguration(new PostMap());
        }
    }
}