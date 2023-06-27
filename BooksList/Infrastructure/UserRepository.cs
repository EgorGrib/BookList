using BooksList.Domain;
using BooksList.DTOs;
using Microsoft.EntityFrameworkCore;
using BCryptNet = BCrypt.Net.BCrypt;

namespace BooksList.Infrastructure;

public class UserRepository : IUserRepository
{
    private readonly BookListDb _dbContext;

    public UserRepository(BookListDb dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<List<User>> GetUsersAsync()
    {
        return _dbContext.Users.Include(u => u.Books).ToListAsync();
    }

    public async ValueTask<User> GetUserAsync(int userId)
    {
        var userFromDb = await _dbContext.Users.Include(u => u.Books)
            .FirstOrDefaultAsync(u => u.Id == userId);

        if (userFromDb == null) throw new InvalidOperationException("User not found");

        return userFromDb;
    }

    public async ValueTask<User> GetUserAsync(LoginUserDto loginUser)
    {
        var userFromDb = await _dbContext.Users.FirstOrDefaultAsync(u =>
            string.Equals(u.Name, loginUser.Name));

        if (userFromDb == null) throw new InvalidOperationException("User not found");
        
        var isMatch = BCryptNet.Verify(loginUser.Password, userFromDb.Password);

        if (!isMatch)
        {
            throw new InvalidOperationException("Incorrect password");
        }

        return userFromDb;
    }
    
    public async Task InsertUserAsync(User user)
    {
        var hashedPassword = BCryptNet.HashPassword(user.Password, BCryptNet.GenerateSalt());
        await _dbContext.AddAsync(new User(user.Id, user.Name, hashedPassword));
    }

    public async Task UpdateUserAsync(User user)
    {
        var userFromDb = await _dbContext.Users.FindAsync(user.Id);

        if (userFromDb == null) throw new InvalidOperationException("User not found");;

        userFromDb.Name = user.Name;
    }

    public async Task DeleteUserAsync(int userId)
    {
        var userFromDb = await _dbContext.Users.FindAsync(userId);
        
        if (userFromDb == null) throw new InvalidOperationException("User not found");;

        _dbContext.Users.Remove(userFromDb);
    }

    public async Task SaveAsync()
    {
        await _dbContext.SaveChangesAsync();
    }
}