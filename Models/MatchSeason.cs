using System.Text.Json.Serialization;

namespace epl_api.Models;

public class MatchSeason
{
    public int Id { get; set; }
    public int MatchId { get; set; }
    public int SeasonId { get; set; }

    [JsonIgnore]
    public virtual Match? Match { get; set; }
    [JsonIgnore]
    public virtual Season? Season { get; set; }
}
