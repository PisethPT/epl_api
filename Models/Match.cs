using System.Text.Json.Serialization;

namespace epl_api.Models;

public class Match
{
    public int Id { get; set; }
    public DateTime MatchDate { get; set; }
    public int HomeTeamScore { get; set; }
    public int AwayTeamScore { get; set; }
    public int HomeTeamId { get; set; }
    public int AwayTeamId { get; set; }
    public int Status { get; set; }

    [JsonIgnore]
    public virtual Team? HomeTeam { get; set; }
    [JsonIgnore]
    public virtual Team? AwayTeam { get; set; }
    [JsonIgnore]
    public virtual ICollection<Assist> Assists { get; set; } = new List<Assist>();
    [JsonIgnore]
    public virtual ICollection<Card> Cards { get; set; } = new List<Card>();
    [JsonIgnore]
    public virtual ICollection<Goal> Goals { get; set; } = new List<Goal>();
    [JsonIgnore]
    public virtual ICollection<MatchSeason> MatchSeasons { get; set; } = new List<MatchSeason>();
}
