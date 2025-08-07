namespace epl_api.DTOs;

public class PlayerDto
{
    public int Id { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Position { get; set; } = string.Empty;
    public int PlayerNumber { get; set; }
    public string SocialMedia { get; set; } = string.Empty;
    public string Nationality { get; set; } = string.Empty;
    public string PreferredFoot { get; set; } = string.Empty;
    public int TeamId { get; set; }
    public string TeamName { get; set; } = string.Empty;

    public IFormFile? Photo { get; set; }
}
