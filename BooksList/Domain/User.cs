namespace BooksList.Domain;

public class User
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Password { get; set; }

    public List<Book> Books { get; set; }

    public User()
    {
        Books = new List<Book>();
    }
    
    public User(int id, string name, string password)
    {
        Id = id;
        Name = name;
        Password = password;
        Books = new List<Book>();
    }
}