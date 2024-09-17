using System.ComponentModel.DataAnnotations.Schema;

namespace ExampleWebPostgresSpecific.Database;

public class Document
{
    public int Id { get; set; }
    public string Name { get; set; }

    [Column(TypeName = "jsonb")]
    public List<Document>? SubDocuments { get; set; }
}
