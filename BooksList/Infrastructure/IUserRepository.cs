using BooksList.Domain;

namespace BooksList.Infrastructure;

interface IUserRepository
{
    Task<List<User>> GetUsersAsync();
    ValueTask<User> GetUserAsync(int userId);
    ValueTask<User> GetUserAsync(User user);
    Task InsertUserAsync(User user);
    Task UpdateUserAsync(User user);
    Task DeleteUserAsync(int userId);
    Task SaveAsync();
} 
