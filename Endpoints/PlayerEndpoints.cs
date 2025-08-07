namespace epl_api.Endpoints;

using Data;
using epl_api.DTOs;
using epl_api.Models;
using epl_api.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

public static class PlayerEndpoints
{
    private static readonly string directory = "Players";
    public static void MapPlayers(this WebApplication app)
    {
        app.MapGet("/players/by-team", async (EPLContext context, int? teamId, int? playerId) =>
        {
            var players = await context.Players.Include(team => team.Team).ToListAsync();
            if (teamId is not null)
                players = players.Where(player => player.TeamId.Equals(teamId)).ToList();
            if (playerId is not null)
            {
                players = players.Where(player => player.Id.Equals(playerId)).ToList();
            }
            return Results.Ok(new
            {
                statusCode = 200,
                message = "Players fetched successfully.",
                content = players.Select(player => new
                {
                    id = player.Id,
                    firstName = player.FirstName,
                    lastName = player.LastName,
                    photo = player.Photo,
                    position = player.Position,
                    playerNumber = player.PlayerNumber,
                    teamId = player.TeamId,
                    teamName = player.Team?.Name,
                    teamClubCrest = player.Team?.ClubCrest,
                    socialMedia = player.SocialMedia,
                    nationality = player.Nationality,
                    preferredFoot = player.PreferredFoot
                }).ToList()
            });
        });

        app.MapGet("/players/{id:int}", async (EPLContext context, int id) =>
        {
            var existingPlayer = await context.Players.FirstOrDefaultAsync(player => player.Id.Equals(id));
            if (existingPlayer is null)
                return Results.NotFound(new
                {
                    statusCode = 404,
                    message = "Player not found."
                });

            return Results.Ok(new
            {
                statusCode = 200,
                message = "Success.",
                content = existingPlayer
            });
        }).RequireAuthorization();

        app.MapPost("/players/create", async (EPLContext context, IFileService fileService, [FromForm] PlayerDto model) =>
        {
            if (model is null)
                return Results.BadRequest("Invalid player data");

            var existingPlayer = await context.Players.FirstOrDefaultAsync(player =>
                                string.Compare(string.Concat(player.FirstName, player.LastName).ToLower(),
                                string.Concat(model.FirstName, model.LastName).ToLower()) == 0 &&
                                player.TeamId.Equals(model.TeamId)
            );

            if (existingPlayer is not null)
                return Results.Conflict("Player is existing.");
            var fileName = string.Empty;
            if (model.Photo is not null)
                fileName = (await fileService.UploadFile(model.Photo!, directory)).Item1 as string;

            var player = new Player
            {
                FirstName = model.FirstName,
                LastName = model.LastName,
                Position = model.Position,
                PlayerNumber = model.PlayerNumber,
                TeamId = model.TeamId,
                Photo = fileName,
                Nationality = model.Nationality,
                PreferredFoot = model.PreferredFoot,
                SocialMedia = model.SocialMedia,
            };
            context.Players.Add(player);
            await context.SaveChangesAsync();
            return Results.Created($"/players/{player.Id}", new
            {
                statusCode = 201,
                message = "Player created successfully.",
                content = player
            });

        }).DisableAntiforgery()
        .RequireAuthorization("OnlyAdmin");

        app.MapPut("/players/edit/{id:int}", async (EPLContext context, IFileService fileService, int id, [FromForm] PlayerDto model) =>
        {
            if (model is null)
                return Results.BadRequest("Invalid player data");
            var duplicatePlayer = await context.Players.FirstOrDefaultAsync(player =>
                                string.Equals(string.Concat(player.FirstName, player.LastName).ToLower(),
                                string.Concat(model.FirstName, model.LastName).ToLower()) &&
                                player.TeamId.Equals(model.TeamId) && player.Id != id
            );

            if (duplicatePlayer is not null)
                return Results.Conflict(new
                {
                    statusCode = 409,
                    message = "Player's information duplication."
                });

            var existingPlayer = await context.Players.FirstOrDefaultAsync(player => player.Id.Equals(id));
            var fileName = string.Empty;
            if (model.Photo is not null)
            {
                var existingFileName = await context.Players.SingleOrDefaultAsync(player => string.Compare(player.Photo, model.Photo.FileName) == 0);
                if (existingFileName is null)
                {
                    fileName = (await fileService.UploadFile(model.Photo, directory)).Item1 as string;
                }
            }

            existingPlayer!.FirstName = string.IsNullOrEmpty(model.FirstName) ? existingPlayer.FirstName : model.FirstName;
            existingPlayer!.LastName = string.IsNullOrEmpty(model.LastName) ? existingPlayer.LastName : model.LastName;
            existingPlayer!.Position = string.IsNullOrEmpty(model.Position) ? existingPlayer.Position : model.Position;
            existingPlayer!.PlayerNumber = model.PlayerNumber == 0 ? existingPlayer.PlayerNumber : model.PlayerNumber;
            existingPlayer!.Nationality = string.IsNullOrEmpty(model.Nationality) ? existingPlayer.Nationality : model.Nationality;
            existingPlayer!.PreferredFoot = string.IsNullOrEmpty(model.PreferredFoot) ? existingPlayer.PreferredFoot : model.PreferredFoot;
            existingPlayer!.SocialMedia = string.IsNullOrEmpty(model.SocialMedia) ? existingPlayer.SocialMedia : model.SocialMedia;
            existingPlayer!.TeamId = model.TeamId;
            if (!string.IsNullOrEmpty(fileName))
                existingPlayer.Photo = fileName;

            context.Players.Update(existingPlayer);
            await context.SaveChangesAsync();
            return Results.Ok(new
            {
                statusCode = 200,
                message = "Player updated successfully.",
                content = existingPlayer
            });
        }).DisableAntiforgery()
        .RequireAuthorization("OnlyAdmin");

        app.MapDelete("/players/delete/{id:int}", async (EPLContext context, int id) =>
        {
            var existingPlayer = await context.Players.SingleOrDefaultAsync(player => player.Id.Equals(id));
            if (existingPlayer is null)
                return Results.BadRequest(new
                {
                    statusCode = 404,
                    message = "Player not found."
                });
            context.Players.Remove(existingPlayer);
            await context.SaveChangesAsync();
            return Results.Ok(new
            {
                statusCode = 200,
                message = "Player deleted successfully."
            });
        }).RequireAuthorization("OnlyAdmin");
    }
}
