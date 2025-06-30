using System.Text.Json.Serialization;

namespace epl_api.Models;

public class Season
{
    public int Id { get; set; }
    /// <summary>
    /// Season name '2024/2025'
    /// </summary> <summary>
    /// 
    /// </summary>
    /// <value>2024/2025</value>
    public required string Name { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }

    [JsonIgnore]
    public virtual ICollection<MatchSeason> MatchSeasons { get; set; } = new List<MatchSeason>();
}
