using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace epl_api.Models;

public class Team
{
    public int Id { get; set; }
    public required string Name { get; set; }
    public required string ClubCrest { get; set; }
    public required string City { get; set; }
    public int Founded { get; set; }
    public required string HomeStadium { get; set; }
    public required string HeadCoach { get; set; }
    public required string TeamThemeColor { get; set; }
    public required string WebsiteUrl { get; set; }

    [JsonIgnore]
    public virtual ICollection<Player> Players { get; set; } = new List<Player>();
    [JsonIgnore]
    public virtual ICollection<Match> HomeMatches { get; set; } = new List<Match>();
    [JsonIgnore]
    public virtual ICollection<Match> AwayMatches { get; set; } = new List<Match>();
    [JsonIgnore]
    public virtual ICollection<Goal> Goals { get; set; } = new List<Goal>();
    [JsonIgnore]
    public virtual ICollection<Assist> Assists { get; set; } = new List<Assist>();
    [JsonIgnore]
    public virtual ICollection<Card> Cards { get; set; } = new List<Card>();
}
