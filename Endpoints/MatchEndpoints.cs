using epl_api.Data;
using epl_api.DTOs;
using epl_api.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace epl_api.Endpoints;

public static class MatchEndpoints
{
    public static void MapMatches(this WebApplication app)
    {
        app.MapGet("/matches", async (EPLContext context) =>
        {
            foreach (var match in context.Matches.Where(match => match.Status != 2))
            {
                if (match.MatchDate.AddMinutes(90) < DateTime.Now && match.Status != 2)
                    match.Status = 2;
                // If the match is ongoing, set status to 0 but alway status is 1 when create first match
                else if (match.MatchDate.AddMinutes(90) > DateTime.Now && match.MatchDate < DateTime.Now && match.Status != 0)
                    match.Status = 0;
                context.Matches.Update(match);
            }
            await context.SaveChangesAsync();

            var matches = await context.Matches.Include(h => h.HomeTeam).Include(a => a.AwayTeam).OrderBy(match => match.Status).ToListAsync();
            if (matches.Count > 0)
            {
                var matchDetails = new List<MatchDetailDto>();
                foreach (var match in matches)
                {
                    var matchDetail = new MatchDetailDto
                    {
                        MatchId = match.Id,
                        MatchDate = match.MatchDate.ToString("dddd, dd-MM-yyyy"),
                        MatchTime = match.MatchDate.ToString("HH:mm tt"),
                        HomeTeamId = match.HomeTeamId,
                        AwayTeamId = match.AwayTeamId,
                        HomeTeamName = match.HomeTeam!.Name,
                        AwayTeamName = match.AwayTeam!.Name,
                        HomeTeamClubCrest = match.HomeTeam.ClubCrest,
                        AwayTeamClubCrest = match.AwayTeam.ClubCrest,
                        HomeTeamScore = match.HomeTeamScore,
                        AwayTeamScore = match.AwayTeamScore,
                        KickoffStadium = match.HomeTeam.HomeStadium,
                        Status = match.Status,
                        IsFinish = match.MatchDate.AddMinutes(90) < DateTime.Now ? true : false
                    };
                    matchDetails.Add(matchDetail);
                }
                return Results.Ok(new
                {
                    statusCode = 200,
                    message = "Success.",
                    data = matchDetails.ToList()
                });
            }
            else
                return Results.Ok(new
                {
                    statusCode = "200",
                    message = "Success.",
                    data = matches
                });
        });

        app.MapGet("/matches/{id:int}", async (EPLContext context, int id) =>
        {
            var match = await context.Matches.Include(h => h.HomeTeam).Include(a => a.AwayTeam)
                .FirstOrDefaultAsync(match => match.Id.Equals(id));
            if (match == null)
            {
                return Results.NotFound(new
                {
                    statusCode = 404,
                    message = "Match not found."
                });
            }
            var matchDetail = new MatchDto
            {
                Id = match.Id,
                MatchDate = match.MatchDate,
                HomeTeamId = match.HomeTeamId,
                AwayTeamId = match.AwayTeamId
            };
            return Results.Ok(new
            {
                statusCode = 200,
                message = "Success.",
                data = matchDetail
            });
        }).RequireAuthorization();

        app.MapPost("/matches/create", async (EPLContext context, [FromForm] MatchDto model) =>
        {
            if (model is null)
                return Results.BadRequest(new
                {
                    statusCode = 404,
                    massage = "Invalid match data."
                });
            var existingMatch = await context.Matches.FirstOrDefaultAsync(match => match.MatchDate.Equals(model.MatchDate) &&
                                            match.HomeTeamId.Equals(model.HomeTeamId) &&
                                            match.AwayTeamId.Equals(model.AwayTeamId));

            if (existingMatch is not null)
                return Results.Conflict(new
                {
                    statusCode = 409,
                    massage = "Match's information duplication.",
                    data = existingMatch
                });
            var match = new Match
            {
                MatchDate = model.MatchDate,
                HomeTeamId = model.HomeTeamId,
                AwayTeamId = model.AwayTeamId,
                Status = 1
            };
            context.Matches.Add(match);
            await context.SaveChangesAsync();
            return Results.Created($"/matches/{match.Id}", new
            {
                statusCode = 201,
                message = "Match created successfully.",
                data = match
            });
        }).DisableAntiforgery()
        .RequireAuthorization("OnlyAdmin");

        app.MapPut("/matches/edit/{id:int}", async (EPLContext context, int Id, [FromForm] MatchDto model) =>
        {
            var matchToEdit = await context.Matches.FindAsync(Id);

            if (matchToEdit == null)
            {
                return Results.NotFound("Match not found.");
            }

            if (matchToEdit.Status == 0)
            {
                return Results.BadRequest(new
                {
                    statusCode = 404,
                    message = "Can be not edit this match, Because this match 🔴 living.",
                });
            }

            if (matchToEdit.Status == 2)
            {
                return Results.BadRequest(new
                {
                    statusCode = 404,
                    message = "Can be not edit this match, Because this ✅ Match Finished",
                });
            }

            // Check for another match with the same HomeTeam and AwayTeam that is either Live or Pending
            var duplicateMatch = await context.Matches.FirstOrDefaultAsync(match =>
                match.HomeTeamId == model.HomeTeamId &&
                match.AwayTeamId == model.AwayTeamId &&
                (match.Status == 0 || match.Status == 2) &&
                match.Id != Id
            );

            if (duplicateMatch is not null)
            {
                return Results.Conflict(new
                {
                    statusCode = 409,
                    message = "A similar match is already scheduled or ongoing.",
                });
            }

            // ... update other fields
            matchToEdit.HomeTeamId = model.HomeTeamId;
            matchToEdit.AwayTeamId = model.AwayTeamId;
            matchToEdit.MatchDate = model.MatchDate;

            context.Matches.Update(matchToEdit);
            await context.SaveChangesAsync();
            return Results.Ok(new
            {
                statusCode = 200,
                message = "Match updated successfully.",
                data = matchToEdit
            });

        }).DisableAntiforgery()
        .RequireAuthorization("OnlyAdmin");

        app.MapDelete("/matches/delete/{id:int}", async (EPLContext context, int id) =>
        {
            var match = await context.Matches.FindAsync(id);
            if (match == null)
            {
                return Results.NotFound(new
                {
                    statusCode = 404,
                    message = "Match not found."
                });
            }
            if (match.Status == 0)
            {
                return Results.BadRequest(new
                {
                    statusCode = 404,
                    message = "Can be not delete this match, Because this match 🔴 living.",
                });
            }
            if (match.Status == 2)
            {
                return Results.BadRequest(new
                {
                    statusCode = 404,
                    message = "Can be not delete this match, Because this ✅ Match Finished",
                });
            }
            context.Matches.Remove(match);
            await context.SaveChangesAsync();
            return Results.Ok(new
            {
                statusCode = 200,
                message = "Match deleted successfully."
            });
        }).DisableAntiforgery()
        .RequireAuthorization("OnlyAdmin");

        // Get ongoing, finished, and upcoming matches
        // Ongoing matches are those that are currently being played (status 0)
        // Finished matches are those that have been completed (status 2)
        // Upcoming matches are those that are scheduled for the future (status 1)
        app.MapGet("/matches/ongoing", async (EPLContext context) =>
        {
            var ongoingMatches = await context.Matches.Include(h => h.HomeTeam).Include(a => a.AwayTeam)
                .Where(match => match.Status == 0)
                .OrderBy(match => match.MatchDate)
                .ToListAsync();

            if (ongoingMatches.Count == 0)
            {
                return Results.Ok(new
                {
                    statusCode = 200,
                    message = "No ongoing matches.",
                    data = new List<MatchDetailDto>()
                });
            }

            var matchDetails = ongoingMatches.Select(match => new MatchDetailDto
            {
                MatchId = match.Id,
                MatchDate = match.MatchDate.ToString("dddd, dd-MM-yyyy"),
                MatchTime = match.MatchDate.ToString("HH:mm tt"),
                HomeTeamId = match.HomeTeamId,
                AwayTeamId = match.AwayTeamId,
                HomeTeamName = match.HomeTeam!.Name,
                AwayTeamName = match.AwayTeam!.Name,
                HomeTeamClubCrest = match.HomeTeam.ClubCrest,
                AwayTeamClubCrest = match.AwayTeam.ClubCrest,
                HomeTeamScore = match.HomeTeamScore,
                AwayTeamScore = match.AwayTeamScore,
                KickoffStadium = match.HomeTeam.HomeStadium,
                Status = match.Status,
                IsFinish = match.MatchDate.AddMinutes(90) < DateTime.Now ? true : false
            }).ToList();

            return Results.Ok(new
            {
                statusCode = 200,
                message = "Success.",
                data = matchDetails
            });
        }).RequireAuthorization();

        app.MapGet("/matches/finished", async (EPLContext context) =>
        {
            var finishedMatches = await context.Matches.Include(h => h.HomeTeam).Include(a => a.AwayTeam)
                .Where(match => match.Status == 2)
                .OrderByDescending(match => match.MatchDate)
                .ToListAsync();

            if (finishedMatches.Count == 0)
            {
                return Results.Ok(new
                {
                    statusCode = 200,
                    message = "No finished matches.",
                    data = new List<MatchDetailDto>()
                });
            }

            var matchDetails = finishedMatches.Select(match => new MatchDetailDto
            {
                MatchId = match.Id,
                MatchDate = match.MatchDate.ToString("dddd, dd-MM-yyyy"),
                MatchTime = match.MatchDate.ToString("HH:mm tt"),
                HomeTeamId = match.HomeTeamId,
                AwayTeamId = match.AwayTeamId,
                HomeTeamName = match.HomeTeam!.Name,
                AwayTeamName = match.AwayTeam!.Name,
                HomeTeamClubCrest = match.HomeTeam.ClubCrest,
                AwayTeamClubCrest = match.AwayTeam.ClubCrest,
                HomeTeamScore = match.HomeTeamScore,
                AwayTeamScore = match.AwayTeamScore,
                KickoffStadium = match.HomeTeam.HomeStadium,
                Status = match.Status,
                IsFinish = match.MatchDate.AddMinutes(90) < DateTime.Now ? true : false
            }).ToList();

            return Results.Ok(new
            {
                statusCode = 200,
                message = "Success.",
                data = matchDetails
            });
        }).RequireAuthorization();

        app.MapGet("/matches/upcoming", async (EPLContext context) =>
        {
            var upcomingMatches = await context.Matches.Include(h => h.HomeTeam).Include(a => a.AwayTeam)
                .Where(match => match.Status == 1 && match.MatchDate > DateTime.Now)
                .OrderBy(match => match.MatchDate)
                .ToListAsync();

            if (upcomingMatches.Count == 0)
            {
                return Results.Ok(new
                {
                    statusCode = 200,
                    message = "No upcoming matches.",
                    data = new List<MatchDetailDto>()
                });
            }

            var matchDetails = upcomingMatches.Select(match => new MatchDetailDto
            {
                MatchId = match.Id,
                MatchDate = match.MatchDate.ToString("dddd, dd-MM-yyyy"),
                MatchTime = match.MatchDate.ToString("HH:mm tt"),
                HomeTeamId = match.HomeTeamId,
                AwayTeamId = match.AwayTeamId,
                HomeTeamName = match.HomeTeam!.Name,
                AwayTeamName = match.AwayTeam!.Name,
                HomeTeamClubCrest = match.HomeTeam.ClubCrest,
                AwayTeamClubCrest = match.AwayTeam.ClubCrest,
                HomeTeamScore = match.HomeTeamScore,
                AwayTeamScore = match.AwayTeamScore,
                KickoffStadium = match.HomeTeam.HomeStadium,
                Status = match.Status,
                IsFinish = match.MatchDate.AddMinutes(90) < DateTime.Now ? true : false
            }).ToList();
            
            return Results.Ok(new
            {
                statusCode = 200,
                message = "Success.",
                data = matchDetails
            });
        }).RequireAuthorization();

    }
}
