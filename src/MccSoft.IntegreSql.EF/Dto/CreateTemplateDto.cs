using System.Text.Json.Serialization;

namespace MccSoft.IntegreSql.EF.Dto;

public partial class CreateTemplateDto
{
    [JsonPropertyName("database")]
    public Database Database { get; set; }
}
