using BooksList.Domain;

namespace BooksList.DTOs;

record BookDto(int Id, string Title, string Author, int Year, List<string> Genre);

record UpdateBookDto(string Title, string Author, int Year, List<string> Genre, ReadStatus Status);

record BookStatusDto(ReadStatus Status);