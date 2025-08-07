using epl_api.Data;
using epl_api.DTOs;
using epl_api.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace epl_api.Endpoints;

public static class CardEndpoints
{
    public static void MapCards(this WebApplication app)
    {
        app.MapGet("/cards", async (EPLContext context, string? query) =>
        {
            var cardsQuery = context.Cards
            .Include(card => card.Match)
                .ThenInclude(match => match!.HomeTeam)
            .Include(card => card.Match)
                .ThenInclude(match => match!.AwayTeam)
            .Include(card => card.Player)
            .Include(card => card.Team)
            .AsQueryable();

            if (!string.IsNullOrWhiteSpace(query))
            {
                var lowerQuery = query.ToLower();
                cardsQuery = cardsQuery.Where(card =>
                    (card.Player != null && (
                    card.Player.FirstName.ToLower().Contains(lowerQuery) ||
                    card.Player.LastName.ToLower().Contains(lowerQuery)
                    )) ||
                    (card.Team != null && card.Team.Name.ToLower().Contains(lowerQuery)) ||
                    (card.Match != null && (
                    (card.Match.HomeTeam != null && card.Match.HomeTeam.Name.ToLower().Contains(lowerQuery)) ||
                    (card.Match.AwayTeam != null && card.Match.AwayTeam.Name.ToLower().Contains(lowerQuery))
                    ))
                );
            }

            var cards = await cardsQuery
            .OrderByDescending(card => card.Id)
            .ToListAsync();

            if (cards.Any(card => card.Match == null))
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
                message = "Cards retrieved successfully",
                length = cards.Count,
                content = cards.Select(card => new
                {
                    card.Id,
                    CardType = card.Type == 0 ? "Yellow" : "Red",
                    card.Minutes,
                    card.MatchId,
                    Match = card.Match == null ? null : new
                    {
                        card.Match.Id,
                        card.Match.HomeTeamId,
                        card.Match.AwayTeamId,
                        card.Match.MatchDate,
                        card.Match.MatchTime,
                        HomeTeamName = card.Match.HomeTeam?.Name ?? "",
                        AwayTeamName = card.Match.AwayTeam?.Name ?? "",
                        HomeTeamClubCrest = card.Match.HomeTeam?.ClubCrest ?? "",
                        AwayTeamClubCrest = card.Match.AwayTeam?.ClubCrest ?? "",
                    },
                    Player = card.Player == null ? null : new
                    {
                        card.Player.Id,
                        card.Player.FirstName,
                        card.Player.LastName,
                        card.Player.Position,
                        card.Player.PlayerNumber,
                        card.Player.Photo,
                    },
                    Team = card.Team == null ? null : new
                    {
                        card.Team.Id,
                        card.Team.Name,
                        card.Team.ClubCrest
                    }
                })
            });
        }).RequireAuthorization();

        app.MapGet("/cards/{id:int}", async (EPLContext context, int id) =>
        {
            var card = await context.Cards.Include(card => card.Match)
                                            .ThenInclude(match => match!.HomeTeam)
                                            .Include(card => card.Match)
                                            .ThenInclude(match => match!.AwayTeam)
                                           .Include(card => card.Player)
                                           .Include(card => card.Team)
                                           .FirstOrDefaultAsync(c => c.Id == id);
            if (card == null)
            {
                return Results.NotFound(new { statusCode = 404, message = "Card not found" });
            }
            return Results.Ok(new
            {
                statusCode = 200,
                message = "Card retrieved successfully",
                content = new
                {
                    card.Id,
                    CardType = card.Type == 0 ? "Yellow" : "Red",
                    card.Minutes,
                    card.MatchId,
                    Match = card.Match == null ? null : new
                    {
                        card.Match.Id,
                        card.Match.HomeTeamId,
                        card.Match.AwayTeamId,
                        card.Match.MatchDate,
                        card.Match.MatchTime,
                        HomeTeamName = card.Match.HomeTeam?.Name ?? "",
                        AwayTeamName = card.Match.AwayTeam?.Name ?? "",
                        HomeTeamClubCrest = card.Match.HomeTeam?.ClubCrest ?? "",
                        AwayTeamClubCrest = card.Match.AwayTeam?.ClubCrest ?? "",
                    },
                    Player = card.Player == null ? null : new
                    {
                        card.Player.Id,
                        card.Player.FirstName,
                        card.Player.LastName,
                        card.Player.Position,
                        card.Player.PlayerNumber,
                        card.Player.Photo,
                    },
                    Team = card.Team == null ? null : new
                    {
                        card.Team.Id,
                        card.Team.Name,
                        card.Team.ClubCrest
                    }
                }
            });
        }).RequireAuthorization();

        app.MapPost("/cards/create", async (EPLContext context, [FromForm] CardDto model) =>
        {
            if (model == null)
                return Results.BadRequest(new
                {
                    statusCode = 400,
                    message = "Invalid card data"
                });

            var existingCard = await context.Cards.FirstOrDefaultAsync(card => card.MatchId == model.MatchId &&
                                                                           card.PlayerId == model.PlayerId &&
                                                                           card.TeamId == model.TeamId && card.Type == model.Type);
            if (existingCard != null)
                return Results.Conflict(new
                {
                    statusCode = 409,
                    message = "Card already exists for this match, player, and team"
                });

            var card = new Card
            {
                Type = model.Type,
                Minutes = model.Minutes,
                MatchId = model.MatchId,
                PlayerId = model.PlayerId,
                TeamId = model.TeamId
            };
            context.Cards.Add(card);
            await context.SaveChangesAsync();
            return Results.Created($"/cards/{card.Id}", new
            {
                statusCode = 201,
                message = "Card created successfully",
                content = card
            });
        }).DisableAntiforgery()
        .RequireAuthorization("OnlyAdmin");

        app.MapPut("/cards/edit/{id:int}", async (EPLContext context, [FromForm] CardDto model, int id) =>
        {
            var existingCard = await context.Cards.FindAsync(id);
            if (existingCard is null)
            {
                return Results.NotFound(new
                {
                    statusCode = 404,
                    message = "Card not found"
                });
            }

            var duplicateCard = await context.Cards.FirstOrDefaultAsync(card => card.MatchId == model.MatchId &&
                                                                           card.PlayerId == model.PlayerId &&
                                                                           card.TeamId == model.TeamId &&
                                                                           card.Type == model.Type &&
                                                                           card.Id != id);
            if (duplicateCard != null)
            {
                return Results.Conflict(new
                {
                    statusCode = 409,
                    message = "Card already exists for this match, player, and team"
                });
            }

            existingCard.Type = model.Type;
            existingCard.Minutes = model.Minutes != 0 ? model.Minutes : existingCard.Minutes;
            existingCard.MatchId = model.MatchId != 0 ? model.MatchId : existingCard.MatchId;
            existingCard.PlayerId = model.PlayerId != 0 ? model.PlayerId : existingCard.PlayerId;
            existingCard.TeamId = model.TeamId != 0 ? model.TeamId : existingCard.TeamId;
            context.Cards.Update(existingCard);
            await context.SaveChangesAsync();

            return Results.Ok(new
            {
                statusCode = 200,
                message = "Card updated successfully",
                content = existingCard,
            });

        }).DisableAntiforgery()
        .RequireAuthorization("OnlyAdmin");

        app.MapDelete("/cards/delete/{id:int}", async (EPLContext context, int id) =>
        {
            var card = await context.Cards.FindAsync(id);
            if (card == null)
            {
                return Results.NotFound(new
                {
                    statusCode = 404,
                    message = "Card not found"
                });
            }

            context.Cards.Remove(card);
            await context.SaveChangesAsync();
            return Results.Ok(new
            {
                statusCode = 200,
                message = "Card deleted successfully"
            });
        }).RequireAuthorization("OnlyAdmin");
    }
}
