using System.Text.Json.Serialization;
using epl_api.Enums;
using Microsoft.AspNetCore.Identity;

namespace epl_api.Models;

public class User : IdentityUser
{
    public required string FirstName { get; set; }
    public required string LastName { get; set; }
    public Gender Gender { get; set; }

    [JsonIgnore]
    public ICollection<News> News { get; set; } = new List<News>();
}
