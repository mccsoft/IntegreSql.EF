namespace ExampleWebPostgresSpecific.Database;

public class User
{
    public int Id { get; set; }
    public string Name { get; set; }
    public List<Document>? Documents { get; set; }
}
