using System.Text.Json.Serialization;

namespace epl_api.Models;

public class Match
{
    public int Id { get; set; }
    public required DateOnly MatchDate { get; set; }
    public required TimeSpan MatchTime { get; set; }
    public int HomeTeamScore { get; set; }
    public int AwayTeamScore { get; set; }
    public required int HomeTeamId { get; set; }
    public required int AwayTeamId { get; set; }
    public int KickoffStatus { get; set; }
    public bool IsGameFinish { get; set; }
    public bool IsHomeStadium { get; set; }

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
