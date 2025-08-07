namespace epl_api.DTOs;

public class MatchDetailDto
{
    public int MatchId { get; set; }
    public int HomeTeamId { get; set; }
    public int AwayTeamId { get; set; }
    public string MatchDateFormat { get; set; } = string.Empty;
    public string MatchTimeFormat  { get; set; } = string.Empty;
    public DateOnly MatchDate { get; set; }
    public TimeSpan MatchTime { get; set; }
    public string HomeTeamName { get; set; } = string.Empty;
    public string AwayTeamName { get; set; } = string.Empty;
    public string HomeTeamClubCrest { get; set; } = string.Empty;
    public string AwayTeamClubCrest { get; set; } = string.Empty;
    public int HomeTeamScore { get; set; }  
    public int AwayTeamScore { get; set; }
    public int KickoffStatus { get; set; }
    public bool IsGameFinish { get; set; }
    public bool IsHomeStadium { get; set; }
    public string KickoffStadium { get; set; } = string.Empty;
}
