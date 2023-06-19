using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

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
    public string Title { get; set; }
    public string Author { get; set; }
    public int Year { get; set; }
    public List<string> Genre { get; set; }
    
    [EnumDataType(typeof(ReadStatus))]
    public ReadStatus Status { get; set; }
    
    [JsonIgnore]
    public User User { get; }
    public int UserId { get; }
    
    public Book(){}

    public Book(int id, User user, string title, string author, int year, List<string> genre, int userId)
    {
        Id = id;
        User = user;
        Title = title;
        Author = author;
        Year = year;
        Genre = genre;
        UserId = userId;
        Status = ReadStatus.ToRead;
    }
}