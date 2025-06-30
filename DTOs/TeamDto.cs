namespace epl_api.DTOs;

public class TeamDto
{
    public string Name { get; set; } = string.Empty;
    public int Founded { get; set; }
    public string City { get; set; } = string.Empty;
    public string HomeStadium { get; set; } = string.Empty;
    public string HeadCoach { get; set; } = string.Empty;

    public IFormFile? ClubCrest { get; set; }
}
