namespace epl_api.DTOs;

public class MatchDto
{
    public int Id { get; set; }
    public DateTime MatchDate { get; set; } = DateTime.Now;
    public int HomeTeamId { get; set; }
    public int AwayTeamId { get; set; }
    public int Status { get; set; }
}
