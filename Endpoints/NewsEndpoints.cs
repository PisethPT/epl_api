using epl_api.Data;
using epl_api.DTOs;
using epl_api.Models;
using epl_api.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace epl_api.Endpoints;

public static class NewsEndpoints
{
    private static readonly string directory = "News";
    public static void MapNews(this WebApplication app)
    {
        app.MapGet("/news", async (EPLContext context) =>
        {
            var news = await context.News.OrderByDescending(news => news.Id).Include(user => user.User).ToListAsync();
            return Results.Ok(new
            {
                statusCode = 200,
                message = "News fetched successfully",
                data = new
                {
                    news = news.Select(n => new
                    {
                        n.Id,
                        n.Title,
                        n.Body,
                        n.PublishedDate,
                        n.Image,
                        n.VideoLink,
                        User = new
                        {
                            n.User!.Id,
                            n.User.FirstName,
                            n.User.LastName,
                            n.User.Email,
                            Gender = n.User.Gender == 0 ? "Male" : "Female"
                        }
                    })
                }
            });
        });

        app.MapGet("/news/{id:int}", async (EPLContext context, int id) =>
        {
            var existingNews = await context.News.Include(user => user.User).FirstOrDefaultAsync(n => n.Id == id);
            if (existingNews is null)
                return Results.NotFound(new
                {
                    statusCode = 404,
                    message = "News not found."
                });

            return Results.Ok(new
            {
                statusCode = 200,
                message = "News fetched successfully.",
                data = new
                {
                    existingNews.Id,
                    existingNews.Title,
                    existingNews.Body,
                    existingNews.PublishedDate,
                    existingNews.Image,
                    existingNews.VideoLink,
                    User = new
                    {
                        existingNews.User!.Id,
                        existingNews.User.FirstName,
                        existingNews.User.LastName,
                        existingNews.User.Email,
                        Gender = existingNews.User.Gender == 0 ? "Male" : "Female"
                    }
                }
            });
        }).RequireAuthorization();

        app.MapPost("/news/create", async (EPLContext context, IFileService fileService, HttpContext httpContext, [FromForm] NewsDto model) =>
        {
            if (model is null)
                return Results.BadRequest(new
                {
                    statusCode = 400,
                    message = "Invalid news data."
                });

            var existingNews = await context.News.FirstOrDefaultAsync(n => n.Title == model.Title);
            if (existingNews is not null)
                return Results.Conflict(new
                {
                    statusCode = 409,
                    message = "A news article with this title already exists."
                });
            string fileName = string.Empty;
            if (model.Image is not null)
                fileName = (await fileService.UploadFile(model.Image, directory)).Item1;

            model.UserId = httpContext.Session.GetString("UserId")!;

            var news = new News
            {
                Title = model.Title,
                Body = model.Body,
                PublishedDate = model.PublishedDate,
                Image = fileName,
                VideoLink = model.VideoLink,
                UserId = model.UserId,
            };
            context.News.Add(news);
            await context.SaveChangesAsync();
            return Results.Created($"/news/{news.Id}", new
            {
                statusCode = 201,
                message = "News created successfully.",
                data = news
            });
        }).DisableAntiforgery()
        .RequireAuthorization("OnlyAdmin");

        app.MapPut("/news/edit/{id:int}", async (EPLContext context, IFileService fileService, HttpContext httpContext, int id, [FromForm] NewsDto model) =>
        {
            if (model is null)
                return Results.BadRequest(new
                {
                    statusCode = 400,
                    message = "Invalid news data."
                });

            var existingNews = await context.News.Include(user => user.User).FirstOrDefaultAsync(n => n.Id == id);
            if (existingNews is null)
                return Results.NotFound(new
                {
                    statusCode = 404,
                    message = "News not found."
                });

            if (existingNews.UserId != httpContext.Session.GetString("UserId"))
                return Results.Forbid();

            var duplicateNews = await context.News
                .Where(n => n.Id != id && n.Title == model.Title)
                .FirstOrDefaultAsync();
            if (duplicateNews is not null)
                return Results.Conflict(new
                {
                    statusCode = 409,
                    message = "A news article with this title already exists."
                });
            string fileName = existingNews.Image!;
            if (model.Image is not null)
            {
                fileName = (await fileService.UploadFile(model.Image, directory)).Item1;
            }

            existingNews.Title = model.Title;
            existingNews.Body = model.Body;
            existingNews.PublishedDate = model.PublishedDate;
            existingNews.Image = fileName;
            existingNews.VideoLink = string.IsNullOrEmpty(model.VideoLink) ? existingNews.VideoLink : model.VideoLink;
            existingNews.UserId = httpContext.Session.GetString("UserId")!;
            context.News.Update(existingNews);
            await context.SaveChangesAsync();
            return Results.Ok(new
            {
                statusCode = 200,
                message = "News updated successfully.",
                data = new
                {
                    existingNews.Id,
                    existingNews.Title,
                    existingNews.Body,
                    existingNews.PublishedDate,
                    existingNews.Image,
                    existingNews.VideoLink,
                    User = new
                    {
                        existingNews.User!.Id,
                        existingNews.User.FirstName,
                        existingNews.User.LastName,
                        existingNews.User.Email,
                        Gender = existingNews.User.Gender == 0 ? "Male" : "Female"
                    }
                }
            });
        }).DisableAntiforgery()
        .RequireAuthorization("OnlyAdmin");

        app.MapDelete("/news/delete/{id:int}", async (EPLContext context, int id) =>
        {
            var existingNews = await context.News.Include(user => user.User).FirstOrDefaultAsync(n => n.Id == id);
            if (existingNews is null)
                return Results.NotFound(new
                {
                    statusCode = 404,
                    message = "News not found."
                });
            context.News.Remove(existingNews);
            await context.SaveChangesAsync();
            return Results.Ok(new
            {
                statusCode = 200,
                message = "News deleted successfully"
            });
        }).DisableAntiforgery()
        .RequireAuthorization("OnlyAdmin");

        app.MapGet("/news/search", async (EPLContext context, string? query) =>
        {
            if (string.IsNullOrWhiteSpace(query))
                return Results.BadRequest(new
                {
                    statusCode = 400,
                    message = "Search query cannot be empty."
                });

            var news = await context.News
                .Where(n => n.Title.Contains(query, StringComparison.OrdinalIgnoreCase) ||
                            n.Body.Contains(query, StringComparison.OrdinalIgnoreCase))
                .OrderByDescending(n => n.Id)
                .Include(user => user.User)
                .ToListAsync();

            if (!news.Any())
                return Results.NotFound(new
                {
                    statusCode = 404,
                    message = "No news articles found matching the search query."
                });
                
            return Results.Ok(new
            {
                statusCode = 200,
                message = "News articles fetched successfully.",
                data = news.Select(n => new
                {
                    n.Id,
                    n.Title,
                    n.Body,
                    n.PublishedDate,
                    n.Image,
                    n.VideoLink,
                    User = new
                    {
                        n.User!.Id,
                        n.User.FirstName,
                        n.User.LastName,
                        n.User.Email,
                        Gender = n.User.Gender == 0 ? "Male" : "Female"
                    }
                })
            });
        }).RequireAuthorization();
    }
}
