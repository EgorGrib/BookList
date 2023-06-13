namespace BooksList.DTOs;

record BookDto(int Id, int UserId, string Title, string Author, int Year, List<string> Genre);