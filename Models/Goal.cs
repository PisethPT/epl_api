using System.Text.Json.Serialization;

namespace epl_api.Models;

public class Goal
{
    public int Id { get; set; }
    public double minutes { get; set; }
    public int MatchId { get; set; }
    public int PlayerId { get; set; }
    public int TeamId { get; set; }

    [JsonIgnore]
    public virtual Match? Match { get; set; }
    [JsonIgnore]
    public virtual Player? Player { get; set; }
    [JsonIgnore]
    public virtual Team? Team { get; set; }
}
