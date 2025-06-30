namespace epl_api.DTOs;

public class NewsDto
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Body { get; set; } = string.Empty;
    public string VideoLink { get; set; } = string.Empty;
    public DateTime PublishedDate { get; set; }
    public string UserId { get; set; } = string.Empty;
    public IFormFile? Image { get; set; }
}
