using BooksList.Domain;

namespace BooksList.Infrastructure;

interface IBookRepository
{
    Task<List<Book>> GetBooksAsync(int userId);
    ValueTask<Book> GetBookAsync(int userId, int bookId);
    Task InsertBookAsync(int userId, Book book);
    Task UpdateBookAsync(int userId, int bookId, Book book);
    Task DeleteBookAsync(int userId, int bookId);
    Task SaveAsync();
}