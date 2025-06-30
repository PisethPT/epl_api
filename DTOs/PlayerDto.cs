namespace epl_api.DTOs;

public class PlayerDto
{
    public int Id { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Position { get; set; } = string.Empty;
    public int PlayerNumber { get; set; }
    public int TeamId { get; set; }

    public IFormFile? Photo { get; set; }
}
