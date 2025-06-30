using System.Text.Json.Serialization;

namespace epl_api.Models;

public class News
{
    public int Id { get; set; }
    public required string Title { get; set; }
    public required string Body { get; set; }
    public string? Image { get; set; }
    public string? VideoLink { get; set; }
    public DateTime PublishedDate { get; set; }
    public required string UserId { get; set; }

    [JsonIgnore]
    public virtual User? User { get; set; }
}
