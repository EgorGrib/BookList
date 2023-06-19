using BooksList.Domain;
using Microsoft.EntityFrameworkCore;

namespace BooksList.Infrastructure;

public class BookRepository : IBookRepository
{
    private readonly BookListDb _dbContext;

    public BookRepository(BookListDb dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<List<Book>> GetBooksAsync(int userId)
    {
        var userFromDb = await _dbContext.Users.Include(u => u.Books)
            .FirstOrDefaultAsync(u => u.Id == userId);
        
        if (userFromDb == null) throw new InvalidOperationException("User not found");

        return userFromDb.Books.ToList();
    }

    public async ValueTask<Book> GetBookAsync(int userId, int bookId)
    {
        var userFromDb = await _dbContext.Users.Include(u => u.Books)
            .FirstOrDefaultAsync(u => u.Id == userId);

        if (userFromDb == null) throw new InvalidOperationException("User not found");
    
        var book = userFromDb.Books.FirstOrDefault(b => b.Id == bookId && b.User.Id == userId);

        if (book == null) throw new InvalidOperationException("Book not found");

        return book;
    }

    public async Task InsertBookAsync(int userId, Book book)
    {
        var userFromDb = await _dbContext.Users.FindAsync(userId);
    
        if (userFromDb == null) throw new InvalidOperationException("User not found");

        _dbContext.Books
            .Add(new Book(book.Id, userFromDb, book.Title, book.Author, book.Year, book.Genre, userId));
    }

    public async Task UpdateBookAsync(int userId, int bookId, Book book)
    {
        var userFromDb = await _dbContext.Users.Include(u => u.Books)
            .FirstOrDefaultAsync(u => u.Id == userId);
    
        if (userFromDb == null) throw new InvalidOperationException("User not found");
    
        var existingBook = userFromDb.Books.FirstOrDefault(b => b.Id == bookId && b.User.Id == userId);
        if (existingBook == null) throw new InvalidOperationException("Book not found");

        existingBook.Author = book.Author;
        existingBook.Genre = book.Genre;
        existingBook.Year = book.Year;
        existingBook.Title = book.Title;
        existingBook.Status = book.Status;
    }

    public async Task DeleteBookAsync(int userId, int bookId)
    {
        var userFromDb = await _dbContext.Users.FindAsync(userId);
        if (userFromDb == null) throw new InvalidOperationException("User not found");
    
        var bookFromDb = await _dbContext.Books.FindAsync(bookId);
        if (bookFromDb == null) throw new InvalidOperationException("Book not found");
    
        _dbContext.Books.Remove(bookFromDb);
    }

    public async Task SaveAsync()
    {
        await _dbContext.SaveChangesAsync();
    }
}