namespace BooksList.DTOs;

public record LoginUserDto(string Name, string Password);

public record UserDto(int Id, string Name, string Password);