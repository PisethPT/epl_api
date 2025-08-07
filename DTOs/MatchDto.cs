namespace epl_api.DTOs;

public class MatchDto
{
    public int Id { get; set; }
    public DateOnly MatchDate { get; set; } = new DateOnly();
    public TimeSpan MatchTime { get; set; } = new TimeSpan();
    public int HomeTeamId { get; set; }
    public int AwayTeamId { get; set; }
    public int HomeTeamScore { get; set; }
    public int AwayTeamScore { get; set; }
    public int KickoffStatus { get; set; }
    public bool IsGameFinish { get; set; }
    public bool IsHomeStadium { get; set; }
}
