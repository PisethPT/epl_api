using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace epl_api.Models;

public class News
{
    public int Id { get; set; }
    [MaxLength(200)]
    public required string Title { get; set; }
    [MaxLength(500)]
    public required string SubTitle { get; set; }
    [MaxLength(4000)]
    public required string Body { get; set; }
    public required string? Image { get; set; }
    public string? VideoLink { get; set; }
    public required DateTime PublishedDate { get; set; }
    public required DateTime ExpireDate { get; set; }
    public bool IsActive { get; set; }
    public required string UserId { get; set; }

    [JsonIgnore]
    public virtual User? User { get; set; }
}
