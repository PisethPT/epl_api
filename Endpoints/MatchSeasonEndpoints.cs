using epl_api.Data;
using epl_api.DTOs;
using epl_api.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace epl_api.Endpoints;

public static class MatchSeasonEndpoints
{
    public static void MapMatchSeason(this WebApplication app)
    {
        app.MapGet("/match-season", async (EPLContext context, string? query) =>
        {
            var matchSeasons = await context.MatchSeasons.Include(match => match.Match)
                                                         .Include(season => season.Season)
                                                         .ToListAsync();
            return Results.Ok(new
            {
                statusCode = 200,
                message = "Match retrieved successfully",
                MatchSeasons = matchSeasons.Select(ms => new
                {
                    ms.MatchId,
                    ms.SeasonId,
                    Match = new
                    {
                        ms.Match!.Id,
                        ms.Match.MatchDate,
                        ms.Match.HomeTeamId,
                        ms.Match.AwayTeamId
                    },
                    Season = new
                    {
                        ms.Season!.Id,
                        ms.Season.Name,
                        ms.Season.StartDate,
                        ms.Season.EndDate
                    }
                })
            });
        });

        app.MapGet("/match-season/{id:int}", async (int id, EPLContext context) =>
        {
            var matchSeason = await context.MatchSeasons.Include(match => match.Match)
                                                        .Include(season => season.Season)
                                                        .FirstOrDefaultAsync(ms => ms.Id == id);
            if (matchSeason == null)
            {
                return Results.NotFound(new
                {
                    statusCode = 404,
                    message = "Match season not found"
                });
            }

            return Results.Ok(new
            {
                statusCode = 200,
                message = "Match retrieved successfully",
                MatchSeason = new
                {
                    matchSeason.MatchId,
                    matchSeason.SeasonId,
                    Match = new
                    {
                        matchSeason.Match!.Id,
                        matchSeason.Match.MatchDate,
                        matchSeason.Match.HomeTeamId,
                        matchSeason.Match.AwayTeamId
                    },
                    Season = new
                    {
                        matchSeason.Season!.Id,
                        matchSeason.Season.Name,
                        matchSeason.Season.StartDate,
                        matchSeason.Season.EndDate
                    }
                }
            });
        }).RequireAuthorization();

        app.MapPost("/match-season/create", async (EPLContext context, [FromForm] MatchSeasonDto model) =>
        {
            if (model is null)
            {
                return Results.BadRequest(new
                {
                    statusCode = 400,
                    message = "Invalid data"
                });
            }

            var existingMatchSeason = await context.MatchSeasons.FirstOrDefaultAsync(ms => ms.SeasonId.Equals(model.SeasonId) && ms.MatchId.Equals(model.MatchId));
            if (existingMatchSeason is not null)
                return Results.Conflict(new
                {
                    statusCode = 409,
                    message = "Match Season already exist for this season, match"
                });

            var matchSeason = new MatchSeason
            {
                MatchId = model.MatchId,
                SeasonId = model.SeasonId
            };

            context.MatchSeasons.Add(matchSeason);
            await context.SaveChangesAsync();
            return Results.Created($"/match-season/{matchSeason.Id}", new
            {
                statusCode = 200,
                message = "Match Season created successfully",
                data = matchSeason
            });

        }).DisableAntiforgery()
        .RequireAuthorization("OnlyAdmin");

        app.MapPut("/match-season/edit/{id:int}", async (EPLContext context, [FromForm] MatchSeasonDto model, int id) =>
        {
            if (model is null)
                return Results.BadRequest(new
                {
                    statusCode = 400,
                    message = "Match Season data is required"
                });

            var existingMatchSeason = await context.MatchSeasons.FindAsync(id);
            if (existingMatchSeason is null)
                return Results.NotFound(new
                {
                    statusCode = 404,
                    message = "Match Season is not found"
                });

            var duplicateMatchSeason = await context.MatchSeasons.SingleOrDefaultAsync(ms => ms.MatchId.Equals(model.MatchId) &&
                                                                                            ms.SeasonId.Equals(model.SeasonId) &&
                                                                                            ms.Id != id);
            if (duplicateMatchSeason is not null)
                return Results.Conflict(new
                {
                    statusCode = 409,
                    message = "Match Season already exist match, season",
                    data = duplicateMatchSeason
                });

            existingMatchSeason.MatchId = model.MatchId;
            existingMatchSeason.SeasonId = model.SeasonId;
            context.Update(existingMatchSeason);
            await context.SaveChangesAsync();
            return Results.Ok(new
            {
                statusCode = 200,
                message = "Match Season updated successfully",
                data = existingMatchSeason
            });
        }).DisableAntiforgery()
        .RequireAuthorization("OnlyAdmin");

        app.MapDelete("/match-season/delete/{id:int}", async (EPLContext context, int id) =>
        {
            var matchSeason = await context.MatchSeasons.FindAsync(id);
            if (matchSeason is null)
                return Results.NotFound(new
                {
                    statusCode = 404,
                    message = "Match Season is not found"
                });
            context.Remove(matchSeason);
            await context.SaveChangesAsync();
            return Results.Ok(new
            {
                statusCode = 200,
                message = "Match Season deleted successfully"
            });
        }).RequireAuthorization("OnlyAdmin");
    }
}
