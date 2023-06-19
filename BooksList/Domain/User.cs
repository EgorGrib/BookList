namespace BooksList.Domain;

public class User
{
    public int Id { get; set; }
    public string Name { get; set; }
    public List<Book> Books { get; set; }

    public User()
    {
        Books = new List<Book>();
    }
    
    public User(int id, string name)
    {
        Id = id;
        Name = name;
        Books = new List<Book>();
    }
}