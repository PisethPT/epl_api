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
            var cards = await context.Cards.Include(card => card.Match)
                                            .Include(card => card.Player)
                                            .Include(card => card.Team)
                                            .OrderByDescending(card => card.Id)
                                            .ToListAsync();
            return Results.Ok(new
            {
                statusCode = 200,
                message = "Cards retrieved successfully",
                length = cards.Count,
                data = cards.Select(card => new
                {
                    card.Id,
                    CardType = card.Type == 0 ? "Yellow" : "Red",
                    card.Minutes,
                    card.MatchId,
                    Match = new
                    {
                        card.Match!.Id,
                        card.Match.HomeTeamId,
                        card.Match.AwayTeamId,
                        card.Match.MatchDate,
                    },
                    Player = new
                    {
                        card.Player!.Id,
                        card.Player.FirstName,
                        card.Player.LastName,
                        card.Player.Position,
                    },
                    Team = new
                    {
                        card.Team!.Id,
                        card.Team.Name,
                        card.Team.ClubCrest
                    }
                })
            });
        });

        app.MapGet("/cards/{id:int}", async (EPLContext context, int id) =>
        {
            var card = await context.Cards.Include(card => card.Match)
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
                data = new
                {
                    card.Id,
                    CardType = card.Type == 0 ? "Yellow" : "Red",
                    card.Minutes,
                    card.MatchId,
                    Match = new
                    {
                        card.Match!.Id,
                        card.Match.HomeTeamId,
                        card.Match.AwayTeamId,
                        card.Match.MatchDate,
                    },
                    Player = new
                    {
                        card.Player!.Id,
                        card.Player.FirstName,
                        card.Player.LastName,
                        card.Player.Position,
                    },
                    Team = new
                    {
                        card.Team!.Id,
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
                data = card
            });
        }).DisableAntiforgery()
        .RequireAuthorization("AdminOnly");

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
            existingCard.Minutes = model.Minutes;
            existingCard.MatchId = model.MatchId;
            existingCard.PlayerId = model.PlayerId;
            existingCard.TeamId = model.TeamId;
            context.Cards.Update(existingCard);
            await context.SaveChangesAsync();

            return Results.Ok(new
            {
                statusCode = 200,
                message = "Card updated successfully",
                data = new
                {
                    existingCard.Id,
                    CardType = existingCard.Type == 0 ? "Yellow" : "Red",
                    existingCard.Minutes,
                    existingCard.MatchId,
                    Match = new
                    {
                        existingCard.Match!.Id,
                        existingCard.Match.HomeTeamId,
                        existingCard.Match.AwayTeamId,
                        existingCard.Match.MatchDate,
                    },
                    Player = new
                    {
                        existingCard.Player!.Id,
                        existingCard.Player.FirstName,
                        existingCard.Player.LastName,
                        existingCard.Player.Position,
                    },
                    Team = new
                    {
                        existingCard.Team!.Id,
                        existingCard.Team.Name,
                        existingCard.Team.ClubCrest
                    }
                }
            });

        }).DisableAntiforgery()
        .RequireAuthorization("AdminOnly");

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
        }).RequireAuthorization("AdminOnly");
    }
}
