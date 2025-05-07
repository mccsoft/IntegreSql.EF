namespace ExampleWebPostgresSpecific.Database;

public enum UserType
{
    Normal,
    Admin
}

public class User
{
    public int Id { get; set; }
    public string Name { get; set; }
    public UserType UserType { get; set; }
    public List<Document>? Documents { get; set; }
}
