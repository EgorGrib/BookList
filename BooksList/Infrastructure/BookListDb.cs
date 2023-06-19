using BooksList.Domain;
using Microsoft.EntityFrameworkCore;

namespace BooksList.Infrastructure;

public class BookListDb : DbContext
{
    public DbSet<User> Users { get; set; } = null!;
    public DbSet<Book> Books { get; set; } = null!;
    
    public BookListDb(DbContextOptions<BookListDb> options) : base(options) {}
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>()
            .HasMany(u => u.Books)
            .WithOne(u => u.User)
            .HasForeignKey(b => b.UserId);


    }
}