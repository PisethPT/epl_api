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
        app.MapGet("/goals", async (EPLContext context) =>
        {
            var goals = await context.Goals.Include(g => g.Match)
                                            .Include(g => g.Player)
                                            .Include(g => g.Team)
                                            .OrderByDescending(g => g.Id)
                                            .ToListAsync();
            return Results.Ok(new
            {
                statusCode = 200,
                message = "Goals retrieved successfully",
                length = goals.Count,
                data = goals.Select(goal => new
                {
                    goal.Id,
                    goal.minutes,
                    goal.MatchId,
                    Match = new
                    {
                        goal.Match!.Id,
                        goal.Match.HomeTeamId,
                        goal.Match.AwayTeamId,
                        goal.Match.MatchDate,
                    },
                    Player = new
                    {
                        goal.Player!.Id,
                        goal.Player.FirstName,
                        goal.Player.LastName,
                        goal.Player.Position,
                    },
                    Team = new
                    {
                        goal.Team!.Id,
                        goal.Team.Name,
                        goal.Team.ClubCrest
                    }
                })
            });
        });

        app.MapGet("/goals/{id:int}", async (EPLContext context, int id) =>
        {
            var goal = await context.Goals.Include(g => g.Match)
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
                data = new
                {
                    goal.Id,
                    goal.minutes,
                    goal.MatchId,
                    Match = new
                    {
                        goal.Match!.Id,
                        goal.Match.HomeTeamId,
                        goal.Match.AwayTeamId,
                        goal.Match.MatchDate,
                    },
                    Player = new
                    {
                        goal.Player!.Id,
                        goal.Player.FirstName,
                        goal.Player.LastName,
                        goal.Player.Position,
                    },
                    Team = new
                    {
                        goal.Team!.Id,
                        goal.Team.Name,
                        goal.Team.ClubCrest
                    }
                }
            });
        }).RequireAuthorization();

        app.MapPost("/goals/create", async(EPLContext context, [FromForm] GoalDto model) =>
        {
            if(model is null)
            {
                return Results.BadRequest(new { statusCode = 400, message = "Invalid goal data" });
            }

            if (model.Minutes < 0 || model.MatchId <= 0 || model.PlayerId <= 0 || model.TeamId <= 0)
            {
                return Results.BadRequest(new { statusCode = 400, message = "Invalid goal data" });
            }
            
            var existingGoal = await context.Goals.FirstOrDefaultAsync(g => g.MatchId == model.MatchId &&
                                                                            g.PlayerId == model.PlayerId &&
                                                                            g.TeamId == model.TeamId &&
                                                                            g.minutes == model.Minutes);
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
                data = goal
            });
        }).DisableAntiforgery()
        .RequireAuthorization("AdminOnly");

        app.MapPut("/goals/edit/{id:int}", async (EPLContext context, [FromBody] GoalDto model, int id) =>
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
                data = goal
            });
        }).DisableAntiforgery()
        .RequireAuthorization("AdminOnly");

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
        }).RequireAuthorization("AdminOnly");
    }
}
