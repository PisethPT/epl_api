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
        app.MapGet("/matches", async (EPLContext context, int? kickoffStatus) =>
        {
            foreach (var match in context.Matches.Where(match => match.KickoffStatus != 2))
            {
                var matchDateTime = match.MatchDate.ToDateTime(TimeOnly.FromTimeSpan(match.MatchTime));
                if (matchDateTime.AddMinutes(90) < DateTime.Now && matchDateTime < DateTime.Now && match.KickoffStatus != 2)
                {
                    match.KickoffStatus = 2;
                    match.IsGameFinish = true;
                }
                // If the match is ongoing, set status to 0 but alway status is 1 when create first match
                else if (matchDateTime.AddMinutes(90) > DateTime.Now && matchDateTime < DateTime.Now && match.KickoffStatus != 0)
                {
                    match.KickoffStatus = 0;
                    match.IsGameFinish = false;
                }
                context.Matches.Update(match);
            }
            await context.SaveChangesAsync();

            var matches = await context.Matches.Include(h => h.HomeTeam).Include(a => a.AwayTeam).OrderBy(match => match.KickoffStatus).ToListAsync();
            if (matches.Count > 0)
            {
                var matchDetails = new List<MatchDetailDto>();
                if (kickoffStatus is not null)
                    matches = matches.Where(match => match.KickoffStatus == kickoffStatus).ToList();

                foreach (var match in matches)
                {
                    var matchDetail = new MatchDetailDto
                    {
                        MatchId = match.Id,
                        MatchDateFormat = match.MatchDate.ToString("dddd, dd-MM-yyyy"),
                        MatchTimeFormat = DateTime.Parse(match.MatchTime.ToString()).ToString("HH:mm tt"),
                        MatchDate = match.MatchDate,
                        MatchTime = match.MatchTime,
                        HomeTeamId = match.HomeTeamId,
                        AwayTeamId = match.AwayTeamId,
                        HomeTeamName = match.HomeTeam!.Name,
                        AwayTeamName = match.AwayTeam!.Name,
                        HomeTeamClubCrest = match.HomeTeam.ClubCrest,
                        AwayTeamClubCrest = match.AwayTeam.ClubCrest,
                        HomeTeamScore = match.HomeTeamScore,
                        AwayTeamScore = match.AwayTeamScore,
                        KickoffStadium = match.IsHomeStadium ? match.HomeTeam!.HomeStadium : match.AwayTeam!.HomeStadium,
                        IsHomeStadium = match.IsHomeStadium ? true : false,
                        KickoffStatus = match.KickoffStatus,
                        IsGameFinish = match.IsGameFinish
                    };
                    matchDetails.Add(matchDetail);
                }
                return Results.Ok(new
                {
                    statusCode = 200,
                    message = "Success.",
                    content = matchDetails.ToList()
                });
            }
            else
                return Results.Ok(new
                {
                    statusCode = "200",
                    message = "Success.",
                    content = matches
                });
        });

        app.MapGet("/matches/{id:int}", async (EPLContext context, int id) =>
        {
            var match = await context.Matches.Include(h => h.HomeTeam).Include(a => a.AwayTeam).FirstOrDefaultAsync(match => match.Id.Equals(id));
            if (match == null)
            {
                return Results.NotFound(new
                {
                    statusCode = 404,
                    message = "Match not found."
                });
            }
            var matchDetail = new MatchDetailDto
            {
                MatchId = match.Id,
                MatchDateFormat = match.MatchDate.ToString("dddd, dd-MM-yyyy"),
                MatchTimeFormat = DateTime.Parse(match.MatchTime.ToString()).ToString("HH:mm tt"),
                MatchDate = match.MatchDate,
                MatchTime = match.MatchTime,
                HomeTeamId = match.HomeTeamId,
                AwayTeamId = match.AwayTeamId,
                HomeTeamName = match.HomeTeam!.Name,
                AwayTeamName = match.AwayTeam!.Name,
                HomeTeamClubCrest = match.HomeTeam.ClubCrest,
                AwayTeamClubCrest = match.AwayTeam.ClubCrest,
                HomeTeamScore = match.HomeTeamScore,
                AwayTeamScore = match.AwayTeamScore,
                KickoffStadium = match.IsHomeStadium ? match.HomeTeam!.HomeStadium : match.AwayTeam!.HomeStadium,
                IsHomeStadium = match.IsHomeStadium ? true : false,
                KickoffStatus = match.KickoffStatus,
                IsGameFinish = match.IsGameFinish
            };
            return Results.Ok(new
            {
                statusCode = 200,
                message = "Success.",
                content = matchDetail
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
                    content = existingMatch
                });
            var match = new Match
            {
                MatchDate = model.MatchDate,
                MatchTime = model.MatchTime,
                HomeTeamId = model.HomeTeamId,
                AwayTeamId = model.AwayTeamId,
                HomeTeamScore = 0,
                AwayTeamScore = 0,
                KickoffStatus = model.KickoffStatus,
                IsHomeStadium = model.IsHomeStadium,
                IsGameFinish = model.IsGameFinish
            };
            context.Matches.Add(match);

            await context.SaveChangesAsync();
            return Results.Created($"/matches/{match.Id}", new
            {
                statusCode = 201,
                message = "Match created successfully.",
                content = match
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
            var matchDateTime = model.MatchDate.ToDateTime(TimeOnly.FromTimeSpan(model.MatchTime));
            if (matchToEdit.KickoffStatus == 0 && matchToEdit.IsGameFinish == false && matchDateTime.AddMinutes(90) > DateTime.Now && matchDateTime < DateTime.Now)
            {
                return Results.BadRequest(new
                {
                    statusCode = 404,
                    message = "Can be not edit this match, Because this match living.",
                });
            }

            if (matchToEdit.KickoffStatus == 2 && matchToEdit.IsGameFinish == true)
            {
                return Results.BadRequest(new
                {
                    statusCode = 404,
                    message = "Can be not edit this match, Because this Match Finished",
                });
            }

            // Check for another match with the same HomeTeam and AwayTeam that is either Live or Pending (kickoff Status 0 is live or 2 is finished)
            var duplicateMatch = await context.Matches.FirstOrDefaultAsync(match =>
                match.HomeTeamId == model.HomeTeamId &&
                match.AwayTeamId == model.AwayTeamId &&
                match.MatchDate == model.MatchDate &&
                (match.KickoffStatus == 0 || match.KickoffStatus == 2) && match.Id != Id
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
            if (matchToEdit.KickoffStatus == 0 && matchToEdit.IsGameFinish == false) // if match is live and not finished
            {
                matchToEdit.KickoffStatus = model.KickoffStatus;
                matchToEdit.IsGameFinish = model.IsGameFinish;
                matchToEdit.HomeTeamScore = model.HomeTeamScore;
                matchToEdit.AwayTeamScore = model.AwayTeamScore;
            }
            else if (matchToEdit.KickoffStatus == 2 && matchToEdit.IsGameFinish == true) // if match is finished
            {
                matchToEdit.KickoffStatus = model.KickoffStatus;
                matchToEdit.IsGameFinish = model.IsGameFinish;
            }
            else if (matchToEdit.KickoffStatus == 1 && matchToEdit.IsGameFinish == false) // if match is upcoming
            {
                matchToEdit.HomeTeamId = model.HomeTeamId;
                matchToEdit.AwayTeamId = model.AwayTeamId;
                matchToEdit.MatchDate = model.MatchDate;
                matchToEdit.MatchTime = model.MatchTime;
                matchToEdit.HomeTeamScore = model.HomeTeamScore;
                matchToEdit.AwayTeamScore = model.AwayTeamScore;
                matchToEdit.IsHomeStadium = model.IsHomeStadium;
                matchToEdit.KickoffStatus = model.KickoffStatus;
                matchToEdit.IsGameFinish = model.IsGameFinish;
            }

            context.Matches.Update(matchToEdit);
            await context.SaveChangesAsync();
            return Results.Ok(new
            {
                statusCode = 200,
                message = "Match updated successfully.",
                content = matchToEdit
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
            if (match.KickoffStatus == 0 && match.IsGameFinish == false)
            {
                return Results.BadRequest(new
                {
                    statusCode = 404,
                    message = "Can be not delete this match, Because this match 🔴 living.",
                });
            }
            if (match.KickoffStatus == 2 && match.IsGameFinish == true)
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

        // Match table
        // About the short abbreviations in Premier League standings (table)
        // Pos Team   Pl  W  D  L  GF  GA  GD  Pts  Next
        app.MapGet("/matches/tables", async (EPLContext context) =>
        {
            foreach (var match in context.Matches.Where(match => match.KickoffStatus != 2))
            {
                var matchDateTime = match.MatchDate.ToDateTime(TimeOnly.FromTimeSpan(match.MatchTime));
                if (matchDateTime.AddMinutes(90) < DateTime.Now && matchDateTime < DateTime.Now && match.KickoffStatus != 2)
                {
                    match.KickoffStatus = 2;
                    match.IsGameFinish = true;
                }
                // If the match is ongoing, set status to 0 but alway status is 1 when create first match
                else if (matchDateTime.AddMinutes(90) > DateTime.Now && matchDateTime < DateTime.Now && match.KickoffStatus != 0)
                {
                    match.KickoffStatus = 0;
                    match.IsGameFinish = false;
                }
                context.Matches.Update(match);
            }
            var teams = await context.Teams.ToListAsync();
            var matches = await context.Matches.ToListAsync();

            var table = teams.Select(team =>
            {
                var played = matches.Count(m => m.HomeTeamId == team.Id || m.AwayTeamId == team.Id);
                var won = matches.Count(m =>
                    (m.HomeTeamId == team.Id && m.HomeTeamScore > m.AwayTeamScore) ||
                    (m.AwayTeamId == team.Id && m.AwayTeamScore > m.HomeTeamScore));
                var drawn = matches.Count(m => m.HomeTeamScore == m.AwayTeamScore &&
                    (m.HomeTeamId == team.Id || m.AwayTeamId == team.Id));
                var lost = played - won - drawn;
                var goalsFor = matches.Where(m => m.HomeTeamId == team.Id).Sum(m => m.HomeTeamScore) +
                           matches.Where(m => m.AwayTeamId == team.Id).Sum(m => m.AwayTeamScore);
                var goalsAgainst = matches.Where(m => m.HomeTeamId == team.Id).Sum(m => m.AwayTeamScore) +
                           matches.Where(m => m.AwayTeamId == team.Id).Sum(m => m.HomeTeamScore);
                var goalDiff = goalsFor - goalsAgainst;
                var points = won * 3 + drawn;

                // Find next match
                var nextMatch = matches
                    .Where(m => (m.HomeTeamId == team.Id || m.AwayTeamId == team.Id) && m.KickoffStatus == 1 && m.IsGameFinish == false)
                    .OrderBy(m => m.MatchDate)
                    .ThenBy(m => m.MatchTime)
                    .FirstOrDefault();

                return new
                {
                    TeamName = team.Name,
                    TeamImage = team.ClubCrest,
                    Pl = played,
                    W = won,
                    D = drawn,
                    L = lost,
                    GF = goalsFor,
                    GA = goalsAgainst,
                    GD = goalDiff,
                    Pts = points,
                    NextMatch = nextMatch != null
                    ? $"{nextMatch.MatchDate:dd-MM-yyyy} {nextMatch.MatchTime} vs {(nextMatch.HomeTeamId == team.Id ? context.Teams.Find(nextMatch.AwayTeamId)?.Name : context.Teams.Find(nextMatch.HomeTeamId)?.Name)}"
                    : "-",
                    NextTeam = nextMatch != null
                    ? $"{(nextMatch.HomeTeamId == team.Id ? context.Teams.Find(nextMatch.AwayTeamId)?.ClubCrest : context.Teams.Find(nextMatch.HomeTeamId)?.ClubCrest)}"
                    : "-"
                };
            }).OrderByDescending(t => t.Pts)
              .ThenByDescending(t => t.GD)
              .ThenByDescending(t => t.GF)
              .ToList();

            return Results.Ok(new
            {
                statusCode = 200,
                message = "Success.",
                content = table
            });
        });

        // Get ongoing, finished, and upcoming matches
        // Ongoing matches are those that are currently being played (status 0)
        // Finished matches are those that have been completed (status 2)
        // Upcoming matches are those that are scheduled for the future (status 1)
        app.MapGet("/matches/ongoing", async (EPLContext context) =>
        {
            var ongoingMatches = await context.Matches.Include(h => h.HomeTeam).Include(a => a.AwayTeam)
                .Where(match => match.KickoffStatus == 0)
                .OrderBy(match => match.MatchDate)
                .ToListAsync();

            if (ongoingMatches.Count == 0)
            {
                return Results.Ok(new
                {
                    statusCode = 200,
                    message = "No ongoing matches.",
                    content = new List<MatchDetailDto>()
                });
            }

            var matchDetails = ongoingMatches.Select(match => new MatchDetailDto
            {
                MatchId = match.Id,
                MatchDateFormat = match.MatchDate.ToString("dddd, dd-MM-yyyy"),
                MatchTimeFormat = DateTime.Parse(match.MatchTime.ToString()).ToString("HH:mm tt"),
                MatchDate = match.MatchDate,
                MatchTime = match.MatchTime,
                HomeTeamId = match.HomeTeamId,
                AwayTeamId = match.AwayTeamId,
                HomeTeamName = match.HomeTeam!.Name,
                AwayTeamName = match.AwayTeam!.Name,
                HomeTeamClubCrest = match.HomeTeam.ClubCrest,
                AwayTeamClubCrest = match.AwayTeam.ClubCrest,
                HomeTeamScore = match.HomeTeamScore,
                AwayTeamScore = match.AwayTeamScore,
                KickoffStadium = match.IsHomeStadium ? match.HomeTeam!.HomeStadium : match.AwayTeam!.HomeStadium,
                IsHomeStadium = match.IsHomeStadium ? true : false,
                KickoffStatus = match.KickoffStatus,
                IsGameFinish = match.IsGameFinish
            }).ToList();

            return Results.Ok(new
            {
                statusCode = 200,
                message = "Success.",
                content = matchDetails
            });
        }).RequireAuthorization();

        app.MapGet("/matches/finished", async (EPLContext context) =>
        {
            var finishedMatches = await context.Matches.Include(h => h.HomeTeam).Include(a => a.AwayTeam)
                .Where(match => match.KickoffStatus == 2)
                .OrderByDescending(match => match.MatchDate)
                .ToListAsync();

            if (finishedMatches.Count == 0)
            {
                return Results.Ok(new
                {
                    statusCode = 200,
                    message = "No finished matches.",
                    content = new List<MatchDetailDto>()
                });
            }

            var matchDetails = finishedMatches.Select(match => new MatchDetailDto
            {
                MatchId = match.Id,
                MatchDateFormat = match.MatchDate.ToString("dddd, dd-MM-yyyy"),
                MatchTimeFormat = DateTime.Parse(match.MatchTime.ToString()).ToString("HH:mm tt"),
                MatchDate = match.MatchDate,
                MatchTime = match.MatchTime,
                HomeTeamId = match.HomeTeamId,
                AwayTeamId = match.AwayTeamId,
                HomeTeamName = match.HomeTeam!.Name,
                AwayTeamName = match.AwayTeam!.Name,
                HomeTeamClubCrest = match.HomeTeam.ClubCrest,
                AwayTeamClubCrest = match.AwayTeam.ClubCrest,
                HomeTeamScore = match.HomeTeamScore,
                AwayTeamScore = match.AwayTeamScore,
                KickoffStadium = match.IsHomeStadium ? match.HomeTeam!.HomeStadium : match.AwayTeam!.HomeStadium,
                IsHomeStadium = match.IsHomeStadium ? true : false,
                KickoffStatus = match.KickoffStatus,
                IsGameFinish = match.IsGameFinish
            }).ToList();

            return Results.Ok(new
            {
                statusCode = 200,
                message = "Success.",
                content = matchDetails
            });
        }).RequireAuthorization();

        app.MapGet("/matches/upcoming", async (EPLContext context) =>
        {
            var upcomingMatches = await context.Matches.Include(h => h.HomeTeam).Include(a => a.AwayTeam)
                .Where(match => match.KickoffStatus == 1 && match.IsGameFinish == false)
                .OrderBy(match => match.MatchDate)
                .ToListAsync();

            if (upcomingMatches.Count == 0)
            {
                return Results.Ok(new
                {
                    statusCode = 200,
                    message = "No upcoming matches.",
                    content = new List<MatchDetailDto>()
                });
            }

            var matchDetails = upcomingMatches.Select(match => new MatchDetailDto
            {
                MatchId = match.Id,
                MatchDateFormat = match.MatchDate.ToString("dddd, dd-MM-yyyy"),
                MatchTimeFormat = DateTime.Parse(match.MatchTime.ToString()).ToString("HH:mm tt"),
                MatchDate = match.MatchDate,
                MatchTime = match.MatchTime,
                HomeTeamId = match.HomeTeamId,
                AwayTeamId = match.AwayTeamId,
                HomeTeamName = match.HomeTeam!.Name,
                AwayTeamName = match.AwayTeam!.Name,
                HomeTeamClubCrest = match.HomeTeam.ClubCrest,
                AwayTeamClubCrest = match.AwayTeam.ClubCrest,
                HomeTeamScore = match.HomeTeamScore,
                AwayTeamScore = match.AwayTeamScore,
                KickoffStadium = match.IsHomeStadium ? match.HomeTeam!.HomeStadium : match.AwayTeam!.HomeStadium,
                IsHomeStadium = match.IsHomeStadium ? true : false,
                KickoffStatus = match.KickoffStatus,
                IsGameFinish = match.IsGameFinish
            }).ToList();

            return Results.Ok(new
            {
                statusCode = 200,
                message = "Success.",
                content = matchDetails
            });
        }).RequireAuthorization();

    }
}
