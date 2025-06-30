namespace epl_api.DTOs;

public class MatchDetailDto
{
    public int MatchId { get; set; }
    public int HomeTeamId { get; set; }
    public int AwayTeamId { get; set; }
    public string MatchDate { get; set; } = string.Empty;
    public string MatchTime { get; set; } = string.Empty;
    public string HomeTeamName { get; set; } = string.Empty;
    public string AwayTeamName { get; set; } = string.Empty;
    public string HomeTeamClubCrest { get; set; } = string.Empty;
    public string AwayTeamClubCrest { get; set; } = string.Empty;
    public int HomeTeamScore { get; set; }
    public int AwayTeamScore { get; set; }
    public string KickoffStadium { get; set; } = string.Empty;
    public int Status { get; set; }
    public bool IsFinish { get; set; }
}
