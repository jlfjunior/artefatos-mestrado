using System.Text.Json.Serialization;

public class BaseEntity
{
    [JsonIgnore]
    public Guid Id { get; set; }
}