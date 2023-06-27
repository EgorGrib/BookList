using BooksList.Domain;
using BooksList.DTOs;

namespace BooksList.Infrastructure;

interface IUserRepository
{
    Task<List<User>> GetUsersAsync();
    ValueTask<User> GetUserAsync(int userId);
    ValueTask<User> GetUserAsync(LoginUserDto loginUser);
    Task InsertUserAsync(User user);
    Task UpdateUserAsync(User user);
    Task DeleteUserAsync(int userId);
    Task SaveAsync();
} 
