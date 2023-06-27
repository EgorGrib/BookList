namespace BooksList.DTOs;

public record UserDto(int Id, string Name, string Password);
public record LoginUserDto(string Name, string Password);