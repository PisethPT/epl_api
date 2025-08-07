using epl_api.Data;
using epl_api.DTOs;
using epl_api.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace epl_api.Endpoints;

public static class TeamEndpoints
{
    public static void MapTeams(this WebApplication app)
    {
        app.MapGet("/teams", LoadTeamsAsync);

        app.MapGet("/teams/{id:int}", async (EPLContext context, int id) =>
        {
            var team = await context.Teams.FirstOrDefaultAsync(team => team.Id.Equals(id));
            return Results.Ok(new
            {
                statusCode = 200,
                message = "Team fetched successfully.",
                data = team
            });
        }).RequireAuthorization();

        app.MapPost("/teams/create", async (EPLContext context, [FromForm] TeamDto model) =>
        {
            if (model is null)
                return Results.BadRequest("Invalid team data.");
            var existingTeam = await context.Teams.FirstOrDefaultAsync(t => t.Name == model.Name);
            if (existingTeam is not null)
                return Results.Conflict("A team with this name already exists.");

            string fileName = string.Empty;
            if (model.ClubCrest is not null)
            {
                fileName = model.ClubCrest.FileName;
                var savePath = Path.Combine("wwwroot", "TeamLogos");
                if (!Directory.Exists(savePath))
                {
                    Directory.CreateDirectory(savePath);
                }
                using (var stream = new FileStream(Path.Combine(savePath, fileName), FileMode.Create))
                {
                    await model.ClubCrest.CopyToAsync(stream);
                }
            }

            var team = new Team
            {
                Name = model.Name,
                Founded = model.Founded,
                City = model.City,
                HomeStadium = model.HomeStadium,
                HeadCoach = model.HeadCoach,
                ClubCrest = fileName,
                WebsiteUrl = model.WebsiteUrl ?? string.Empty
            };
            context.Teams.Add(team);
            await context.SaveChangesAsync();
            return Results.Created($"/teams/{team.Id}", new
            {
                statusCode = 201,
                message = "Team created successfully.",
                data = team
            });
        }).DisableAntiforgery()
        .RequireAuthorization("OnlyAdmin");

        app.MapPut("/teams/edit/{id:int}", async (EPLContext context, int id, [FromForm] TeamDto model) =>
        {
            var existingTeam = await context.Teams.SingleOrDefaultAsync(team => team.Id.Equals(id));
            if (existingTeam is null)
                return Results.NotFound("Team not found.");

            string fileName = string.Empty;
            if (model.ClubCrest is not null)
            {
                var existingClubCrest = await context.Teams.FirstOrDefaultAsync(team => string.Compare(team.ClubCrest, model.ClubCrest.FileName) == 0);
                if (existingClubCrest is null)
                {
                    fileName = model.ClubCrest.FileName;
                    var savePath = Path.Combine("wwwroot", "TeamLogos");
                    if (!Directory.Exists(savePath))
                    {

                        Directory.CreateDirectory(savePath);
                    }
                    using (var stream = new FileStream(Path.Combine(savePath, fileName), FileMode.Create))
                    {
                        await model.ClubCrest.CopyToAsync(stream);
                    }
                }
            }

            existingTeam.Name = string.IsNullOrEmpty(model.Name) ? existingTeam.Name : model.Name;
            existingTeam.Founded = model.Founded == 0 ? existingTeam.Founded : model.Founded;
            existingTeam.City = string.IsNullOrEmpty(model.City) ? existingTeam.City : model.City;
            existingTeam.HomeStadium = string.IsNullOrEmpty(model.HomeStadium) ? existingTeam.HomeStadium : model.HomeStadium;
            existingTeam.HeadCoach = string.IsNullOrEmpty(model.HeadCoach) ? existingTeam.HeadCoach : model.HeadCoach;
            existingTeam.WebsiteUrl = string.IsNullOrEmpty(model.WebsiteUrl) ? existingTeam.WebsiteUrl : model.WebsiteUrl;
            if (!string.IsNullOrEmpty(fileName))
                existingTeam.ClubCrest = fileName;

            context.Teams.Update(existingTeam);
            await context.SaveChangesAsync();
            return Results.Ok(new
            {
                statusCode = 200,
                message = "Team updated successfully.",
                data = existingTeam
            });

        }).DisableAntiforgery()
        .RequireAuthorization("OnlyAdmin");

        app.MapDelete("/teams/delete/{id:int}", async (EPLContext context, int id) =>
        {
            var existingTeam = await context.Teams.SingleOrDefaultAsync(team => team.Id.Equals(id));
            if (existingTeam is null)
                return Results.NotFound("Team not found.");

            context.Teams.Remove(existingTeam);
            await context.SaveChangesAsync();
            return Results.Ok(new
            {
                statusCode = 200,
                message = "Team deleted successfully."
            });
        }).RequireAuthorization("OnlyAdmin");
    }
    private static async Task<IResult> LoadTeamsAsync(EPLContext context, int? id, string? search, int? delay)
    {
        var teams = await context.Teams.ToListAsync();
        if (id > 0)
            teams = teams.Where(team => team.Id.Equals(id)).ToList();

        if (!string.IsNullOrEmpty(search))
        {
            teams = teams?.Where(team => team.Name.Contains(search, StringComparison.OrdinalIgnoreCase) ||
                                team.Founded.ToString().Contains(search, StringComparison.OrdinalIgnoreCase) ||
                                team.City.Contains(search, StringComparison.OrdinalIgnoreCase) ||
                                team.HomeStadium.Contains(search, StringComparison.OrdinalIgnoreCase) ||
                                team.HeadCoach.Contains(search, StringComparison.OrdinalIgnoreCase)
            ).ToList();
        }

        if (delay is not null)
        {
            if (delay > 300000)
                delay = 300000;
        }

        await Task.Delay(delay ?? 0);
        return Results.Ok(teams);
    }
}
