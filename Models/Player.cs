using System.Text.Json.Serialization;

namespace epl_api.Models;

public class Player
{
    public int Id { get; set; }
    public required string FirstName { get; set; }
    public required string LastName { get; set; }
    public string? Photo { get; set; }
    public required string Position { get; set; }
    public int PlayerNumber { get; set; }
    public int TeamId { get; set; }

    [JsonIgnore]
    public virtual Team? Team { get; set; }

    [JsonIgnore]
    public virtual ICollection<Assist> Assists { get; set; } = new List<Assist>();
    [JsonIgnore]
    public virtual ICollection<Card> Cards { get; set; } = new List<Card>();
    [JsonIgnore]
    public virtual ICollection<Goal> Goals { get; set; } = new List<Goal>();
}
