namespace BooksList.Domain;

public enum ReadStatus
{
    ToRead,
    InProgress,
    Completed
}

public class Book
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public string Title { get; set; }
    public string Author { get; set; }
    public int Year { get; set; }
    public List<string> Genre { get; set; }
    public ReadStatus status { get; set; }

    public Book(int id, int userId, string title, string author, int year, List<string> genre)
    {
        Id = id;
        UserId = userId;
        Title = title;
        Author = author;
        Year = year;
        Genre = genre;
        status = ReadStatus.ToRead;
    }
}