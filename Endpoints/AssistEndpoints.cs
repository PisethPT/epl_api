using epl_api.Data;
using epl_api.DTOs;
using epl_api.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace epl_api.Endpoints;

public static class AssistEndpoints
{
    public static void MapAssists(this WebApplication app)
    {
        app.MapGet("/assists", async (EPLContext context, string? query) =>
        {
            var assists = await context.Assists.Include(match => match.Match)
                                               .Include(player => player.Player)
                                               .Include(team => team.Team).ToListAsync();
            return Results.Ok(new
            {
                statusCode = 200,
                message = "Assists retrieved successfully",
                data = assists.Select(assist => new
                {
                    assist.Id,
                    assist.MatchId,
                    assist.PlayerId,
                    assist.TeamId,
                    assist.Minutes,
                    assist.Match!.MatchDate,
                    PlayerName = string.Concat(assist.Player!.FirstName, " ", assist.Player.LastName),
                    assist.Player.PlayerNumber,
                    assist.Team!.Name,
                    assist.Team.ClubCrest
                })
            });
        });

        app.MapPost("/assists/create", async (EPLContext context, [FromForm] AssistDto model) =>
        {
            if (model is null)
                return Results.BadRequest(new
                {
                    statusCode = 400,
                    message = "Assist data is required"
                });
            var existingAssist = await context.Assists.FirstOrDefaultAsync(a => a.MatchId == model.MatchId && a.PlayerId == model.PlayerId && a.TeamId == model.TeamId);
            if (existingAssist is not null)
                return Results.Conflict(new
                {
                    statusCode = 409,
                    message = "Assist already exists for this match, player, and team"
                });
            var assist = new Assist
            {
                Minutes = model.Minutes,
                MatchId = model.MatchId,
                PlayerId = model.PlayerId,
                TeamId = model.TeamId
            };
            context.Assists.Add(assist);
            await context.SaveChangesAsync();
            return Results.Created($"/assists/{assist.Id}", new
            {
                statusCode = 201,
                message = "Assist created successfully",
                data = assist
            });
        }).DisableAntiforgery()
        .RequireAuthorization("OnlyAdmin");

        app.MapGet("/assists/{id:int}", async (EPLContext context, int id) =>
        {
            var assist = await context.Assists.Include(match => match.Match)
                                              .Include(player => player.Player)
                                              .Include(team => team.Team)
                                              .FirstOrDefaultAsync(a => a.Id == id);
            if (assist is null)
                return Results.NotFound(new
                {
                    statusCode = 404,
                    message = "Assist not found"
                });
            return Results.Ok(new
            {
                statusCode = 200,
                message = "Assist retrieved successfully",
                data = new
                {
                    assist.Id,
                    assist.MatchId,
                    assist.PlayerId,
                    assist.TeamId,
                    assist.Minutes,
                    assist.Match!.MatchDate,
                    PlayerName = string.Concat(assist.Player!.FirstName, " ", assist.Player.LastName),
                    assist.Player.PlayerNumber,
                    assist.Team!.Name,
                    assist.Team.ClubCrest
                }
            });
        }).RequireAuthorization();

        app.MapPut("/assists/edit/{id:int}", async (EPLContext context, int id, [FromForm] AssistDto model) =>
        {
            if (model is null)
                return Results.BadRequest(new
                {
                    statusCode = 400,
                    message = "Assist data is required"
                });

            var existingAssist = await context.Assists.FindAsync(id);
            if (existingAssist is null)
                return Results.NotFound(new
                {
                    statusCode = 404,
                    message = "Assist not found"
                });

            var duplicateAssist = await context.Assists.FirstOrDefaultAsync(a => a.MatchId == model.MatchId && a.PlayerId == model.PlayerId && a.TeamId == model.TeamId && a.Id != id);
            if (duplicateAssist is not null)
                return Results.Conflict(new
                {
                    statusCode = 409,
                    message = "Assist already exists for this match, player, and team"
                });

            existingAssist.Minutes = model.Minutes;
            existingAssist.MatchId = model.MatchId;
            existingAssist.PlayerId = model.PlayerId;
            existingAssist.TeamId = model.TeamId;

            context.Assists.Update(existingAssist);
            await context.SaveChangesAsync();
            return Results.Ok(new
            {
                statusCode = 200,
                message = "Assist updated successfully",
                data = existingAssist
            });
        }).DisableAntiforgery()
        .RequireAuthorization("OnlyAdmin");

        app.MapDelete("/assists/delete/{id:int}", async (EPLContext context, int id) =>
        {
            var assist = await context.Assists.FindAsync(id);
            if (assist is null)
                return Results.NotFound(new
                {
                    statusCode = 404,
                    message = "Assist not found"
                });
            context.Assists.Remove(assist);
            await context.SaveChangesAsync();
            return Results.Ok(new
            {
                statusCode = 200,
                message = "Assist deleted successfully"
            });
        }).RequireAuthorization("AdminOnly");
    }
}
