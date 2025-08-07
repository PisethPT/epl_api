using epl_api.Data;
using epl_api.DTOs;
using epl_api.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace epl_api.Endpoints;

public static class SeasonEndpoints
{
    public static void MapSeasons(this WebApplication app)
    {
        app.MapGet("/seasons", async (EPLContext context, string? query) =>
        {
            var seasons = await context.Seasons.ToListAsync();
            if (!string.IsNullOrEmpty(query))
                seasons = seasons.Where(season => season.Name.Contains(query!, StringComparison.OrdinalIgnoreCase) ||
                                                season.StartDate.Year.ToString().Contains(query!, StringComparison.OrdinalIgnoreCase) ||
                                                season.EndDate.Year.ToString().Contains(query!, StringComparison.OrdinalIgnoreCase)).ToList();
            return Results.Ok(new
            {
                statusCode = 200,
                message = "Seasons retrieved successfully",
                content = seasons
            });
        });

        app.MapGet("/seasons/{id:int}", async (EPLContext context, int id) =>
        {
            var season = await context.Seasons.FindAsync(id);
            if (season is null)
                return Results.NotFound(new
                {
                    statusCode = 404,
                    message = "Season is not found"
                });

            return Results.Ok(new
            {
                statusCode = 200,
                message = "Season retrieved is successfully",
                content = season
            });
        }).RequireAuthorization();

        app.MapPost("/seasons/create", async (EPLContext context, [FromForm] SeasonDto model) =>
        {
            if (model is null)
                return Results.BadRequest(new
                {
                    statusCode = 400,
                    message = "Season data is required"
                });

            if (model.StartDate.Year >= model.EndDate.Year)
                return Results.BadRequest(new
                {
                    statusCode = 400,
                    message = "Start year of season is current year, and must be smaller than End of year of this season"
                });

            if (model.StartDate.Year < model.EndDate.Year - 1)
                return Results.BadRequest(new
                {
                    statusCode = 400,
                    message = "End year of season must be next of Start year of season"
                });

            var existingSeason = await context.Seasons.FirstOrDefaultAsync(season => string.Compare(season.Name.ToLower(), model.Name.ToLower()) == 1 &&
                                                                                    season.StartDate.Year.Equals(model.StartDate.Year) &&
                                                                                    season.EndDate.Year.Equals(model.EndDate.Year));
            if (existingSeason is not null)
                return Results.Conflict(new
                {
                    statusCode = 409,
                    message = "This season is already exist season",
                    content = existingSeason
                });

            var season = new Season
            {
                Name = model.Name,
                StartDate = model.StartDate,
                EndDate = model.EndDate
            };
            context.Seasons.Add(season);
            await context.SaveChangesAsync();
            return Results.Created($"/seasons/{season.Id}", new
            {
                statusCode = 201,
                message = "Season created successfully",
                content = season
            });
        }).DisableAntiforgery()
        .RequireAuthorization("OnlyAdmin");

        app.MapPut("/seasons/edit/{id:int}", async (EPLContext context, [FromForm] SeasonDto model, int id) =>
        {
            if (model is null)
                return Results.BadRequest(new
                {
                    statusCode = 400,
                    message = "Season data is required"
                });

            var existingSeason = await context.Seasons.FindAsync(id);
            if (existingSeason is null)
                return Results.NotFound(new
                {
                    statusCode = 404,
                    message = "Season is not found"
                });

            var duplicateSeason = await context.Seasons.FirstOrDefaultAsync(season => string.Compare(season.Name.ToLower(), model.Name.ToLower()) == 1 &&
                                                                                    season.StartDate.Year.Equals(model.StartDate.Year) &&
                                                                                    season.EndDate.Year.Equals(model.EndDate.Year) &&
                                                                                    season.Id != id);
            if (duplicateSeason is not null)
                return Results.Conflict(new
                {
                    statusCode = 409,
                    message = "Season already exist name, start year or end year of season",
                    content = duplicateSeason
                });

            existingSeason.Name = string.IsNullOrEmpty(model.Name) ? existingSeason.Name : model.Name;
            existingSeason.StartDate = string.IsNullOrEmpty(model.StartDate.ToString()) ? existingSeason.StartDate : model.StartDate;
            existingSeason.EndDate = string.IsNullOrEmpty(model.EndDate.ToString()) ? existingSeason.EndDate : model.EndDate;
            context.Update(existingSeason);
            await context.SaveChangesAsync();

            return Results.Ok(new
            {
                statusCode = 200,
                message = "Season updated successfully",
                content = existingSeason
            });
        }).DisableAntiforgery()
        .RequireAuthorization("OnlyAdmin");

        app.MapDelete("/seasons/delete/{id:int}", async (EPLContext context, int id) =>
        {
            var season = await context.Seasons.FindAsync(id);
            if (season is null)
                return Results.NotFound(new
                {
                    statusCode = 404,
                    message = "Season data is not found"
                });

            context.Seasons.Remove(season);
            await context.SaveChangesAsync();
            return Results.Ok(new
            {
                statusCode = 200,
                message = "Season deleted successfully"
            });
        }).RequireAuthorization("OnlyAdmin");
    }
}
