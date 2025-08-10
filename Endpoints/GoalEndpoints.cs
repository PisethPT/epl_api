using epl_api.Data;
using epl_api.DTOs;
using epl_api.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace epl_api.Endpoints;

public static class GoalEndpoints
{
    public static void MapGoals(this WebApplication app)
    {
        MapGetGoalsEndpoint(app);
        MapGetGoalByIdEndpoint(app);
        MapCreateGoalEndpoint(app);
        MapEditGoalEndpoint(app);
        MapDeleteGoalEndpoint(app);
    }

    private static void MapGetGoalsEndpoint(WebApplication app)
    {
        app.MapGet("/goals", async (EPLContext context, string? query) =>
        {
            var goalsQuery = GetGoalsBaseQuery(context);

            goalsQuery = ApplyGoalFilter(goalsQuery, query);

            var goals = await goalsQuery.OrderByDescending(goal => goal.Id).ToListAsync();

            if (goals.Any(goal => goal.Match == null))
            {
                return Results.BadRequest(new
                {
                    statusCode = 400,
                    message = "One or more cards have a missing match."
                });
            }

            return Results.Ok(new
            {
                statusCode = 200,
                message = "Goals retrieved successfully",
                length = goals.Count,
                content = goals.Select(ProjectGoalToDto)
            });
        }).RequireAuthorization();
    }


    private static void MapGetGoalByIdEndpoint(WebApplication app)
    {
        app.MapGet("/goals/{id:int}", async (EPLContext context, int id) =>
        {
            var goal = await context.Goals.Include(g => g.Match)
                                            .ThenInclude(match => match!.HomeTeam)
                                          .Include(g => g.Match)
                                            .ThenInclude(match => match!.AwayTeam)
                                          .Include(g => g.Player)
                                          .Include(g => g.Team)
                                          .FirstOrDefaultAsync(g => g.Id == id);
            if (goal == null)
            {
                return Results.NotFound(new { statusCode = 404, message = "Goal not found" });
            }
            return Results.Ok(new
            {
                statusCode = 200,
                message = "Goal retrieved successfully",
                content = ProjectGoalToDto(goal)
            });
        }).RequireAuthorization();
    }

    private static void MapCreateGoalEndpoint(WebApplication app)
    {
        _ = app.MapPost("/goals/create", async (EPLContext context, [FromForm] GoalDto model) =>
        {
            if (model is null)
            {
                return Results.BadRequest(new { statusCode = 400, message = "Invalid goal data" });
            }

            if (model.Minutes < 0 || model.MatchId <= 0 || model.PlayerId <= 0 || model.TeamId <= 0)
            {
                return Results.BadRequest(new { statusCode = 400, message = "Invalid goal data" });
            }

            const double epsilon = 0.0001;
            var existingGoal = await context.Goals.FirstOrDefaultAsync(g => g.MatchId == model.MatchId &&
                                                                            g.PlayerId == model.PlayerId &&
                                                                            g.TeamId == model.TeamId &&
                                                                            Math.Abs(g.minutes - model.Minutes) < epsilon);
            if (existingGoal != null)
            {
                return Results.BadRequest(new
                {
                    statusCode = 400,
                    message = "Goal already exists for this match, player, and team at the specified minute"
                });
            }

            var goal = new Goal
            {
                minutes = model.Minutes,
                MatchId = model.MatchId,
                PlayerId = model.PlayerId,
                TeamId = model.TeamId
            };

            context.Goals.Add(goal);
            await context.SaveChangesAsync();

            return Results.Created($"/goals/{goal.Id}", new
            {
                statusCode = 201,
                message = "Goal created successfully",
                content = goal
            });
        }).DisableAntiforgery()
        .RequireAuthorization("OnlyAdmin");
    }

    private static void MapEditGoalEndpoint(WebApplication app)
    {
        app.MapPut("/goals/edit/{id:int}", async (EPLContext context, [FromForm] GoalDto model, int id) =>
        {
            var goal = await context.Goals.FindAsync(id);
            if (goal == null)
            {
                return Results.NotFound(new { statusCode = 404, message = "Goal not found" });
            }

            goal.minutes = model.Minutes;
            goal.MatchId = model.MatchId;
            goal.PlayerId = model.PlayerId;
            goal.TeamId = model.TeamId;

            context.Goals.Update(goal);
            await context.SaveChangesAsync();

            return Results.Ok(new
            {
                statusCode = 200,
                message = "Goal updated successfully",
                content = goal
            });
        }).DisableAntiforgery()
        .RequireAuthorization("OnlyAdmin");
    }

    private static void MapDeleteGoalEndpoint(WebApplication app)
    {
        app.MapDelete("/goals/delete/{id:int}", async (EPLContext context, int id) =>
        {
            var goal = await context.Goals.FindAsync(id);
            if (goal == null)
            {
                return Results.NotFound(new { statusCode = 404, message = "Goal not found" });
            }

            context.Goals.Remove(goal);
            await context.SaveChangesAsync();

            return Results.Ok(new
            {
                statusCode = 200,
                message = "Goal deleted successfully"
            });
        }).RequireAuthorization("OnlyAdmin");
    }

    // filtering and temporary methods
    private static IQueryable<Goal> GetGoalsBaseQuery(EPLContext context)
    {
        return context.Goals.Include(goal => goal.Match)
                                .ThenInclude(match => match!.HomeTeam)
                            .Include(goal => goal.Match)
                                .ThenInclude(match => match!.AwayTeam)
                            .Include(goal => goal.Player)
                            .Include(goal => goal.Team)
                            .OrderByDescending(goal => goal.Id)
                            .AsQueryable();
    }

    private static IQueryable<Goal> ApplyGoalFilter(IQueryable<Goal> goalsQuery, string? query)
    {
        if (string.IsNullOrWhiteSpace(query))
            return goalsQuery;

        var lowerQuery = query.ToLower();
        return goalsQuery.Where(goal =>
            (goal.Player != null && (goal.Player.FirstName.ToLower().Contains(lowerQuery) || goal.Player.LastName.ToLower().Contains(lowerQuery))) ||
            (goal.Team != null && goal.Team.Name.ToLower().Contains(lowerQuery)) ||
            (goal.Match != null && (
                (goal.Match.HomeTeam != null && goal.Match.HomeTeam.Name.ToLower().Contains(lowerQuery)) ||
                (goal.Match.AwayTeam != null && goal.Match.AwayTeam.Name.ToLower().Contains(lowerQuery))
            ))
        );
    }

    private static object ProjectGoalToDto(Goal goal)
    {
        return new
        {
            goal.Id,
            goal.minutes,
            goal.MatchId,
            Match = goal.Match == null ? null : new
            {
                goal.Match!.Id,
                goal.Match.HomeTeamId,
                goal.Match.AwayTeamId,
                goal.Match.MatchDate,
                goal.Match.MatchTime,
                HomeTeamName = goal.Match.HomeTeam?.Name ?? "",
                AwayTeamName = goal.Match.AwayTeam?.Name ?? "",
                HomeTeamScore = goal.Match.HomeTeamScore,
                AwayTeamScore = goal.Match.AwayTeamScore,
                HomeTeamClubCrest = goal.Match.HomeTeam?.ClubCrest ?? "",
                AwayTeamClubCrest = goal.Match.AwayTeam?.ClubCrest ?? "",
            },
            Player = goal.Player == null ? null : new
            {
                goal.Player!.Id,
                goal.Player.FirstName,
                goal.Player.LastName,
                goal.Player.Position,
                goal.Player.PlayerNumber,
                goal.Player.Photo,
            },
            Team = goal.Team == null ? null : new
            {
                goal.Team!.Id,
                goal.Team.Name,
                goal.Team.ClubCrest
            }
        };
    }
}
