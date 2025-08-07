using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using epl_api.DTOs;
using epl_api.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;

namespace epl_api.Endpoints;

public static class RootEndpoints
{
    public static void MapRoots(this WebApplication app, IConfiguration configuration)
    {
        app.Map("/", () => Results.Redirect("/scalar/v1"));

        app.MapPost("/register", async (UserManager<User> userManager, RoleManager<IdentityRole> roleManager, [FromForm] RegisterDto model) =>
        {
            if (model is null)
                return Results.BadRequest("Invalid user data");
            if (string.IsNullOrEmpty(model.Role))
                model.Role = "Guest";
            var user = new User
            {
                FirstName = model.FirstName,
                LastName = model.LastName,
                Gender = model.Gender,
                UserName = string.Concat(model.FirstName, model.LastName),
                Email = model.Email
            };

            var existingUser = await userManager.FindByEmailAsync(model.Email);
            if (existingUser is not null)
                return Results.Conflict("User already register, Please login.");

            var result = await userManager.CreateAsync(user, model.Password);
            if (!result.Succeeded)
                return Results.BadRequest(result.Errors);

            if (!await roleManager.RoleExistsAsync(model.Role))
                await roleManager.CreateAsync(new IdentityRole(model.Role));
            await userManager.AddToRoleAsync(user, model.Role);

            return Results.Ok(new
            {
                statusCode = 200,
                message = "User registered successfully",
                data = new
                {
                    userName = string.Concat(user.FirstName, " ", user.LastName),
                    email = user.Email,
                    role = model.Role
                }
            });
        }).DisableAntiforgery();

        app.MapPost("/login", async (UserManager<User> userManager, HttpContext httpContext, [FromForm] LoginDto model) =>
        {
            var user = await userManager.FindByEmailAsync(model.Email);
            if (user is not null && await userManager.CheckPasswordAsync(user, model.Password))
            {
                var userRole = await userManager.GetRolesAsync(user);
                var authClaims = new List<Claim>
                {
                    new Claim(ClaimTypes.Name, user.UserName!),
                    new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
                };
                foreach (var role in userRole)
                {
                    authClaims.Add(new Claim(ClaimTypes.Role, role));
                }
                var token = GetToken(configuration, authClaims);

                // // Set the token in a secure, HttpOnly cookie
                // httpContext.Response.Cookies.Append("X-Access-Token", new JwtSecurityTokenHandler().WriteToken(token), new CookieOptions
                // {
                //     HttpOnly = true,
                //     Secure = true,
                //     SameSite = SameSiteMode.Strict,
                //     Expires = token.ValidTo
                // });

                // Set the user session
                httpContext.Session.SetString("UserId", user.Id);
                httpContext.Session.SetString("UserName", user.UserName!);
                httpContext.Session.SetString("UserRole", string.Join(",", userRole));

                return Results.Ok(new
                {
                    token = new JwtSecurityTokenHandler().WriteToken(token),
                    expiration = token.ValidTo,
                    userName = string.Concat(user.FirstName, " ", user.LastName),
                    userId = httpContext.Session.GetString("UserId")
                });
            }
            return Results.Unauthorized();
        }).DisableAntiforgery();

    }

    private static JwtSecurityToken GetToken(IConfiguration configuration, List<Claim> authClaims)
    {
        var authSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration!["Jwt:Key"]!));

        return new JwtSecurityToken(
            issuer: configuration["Jwt:Issuer"],
            audience: configuration["Jwt:Audience"],
            expires: DateTime.UtcNow.AddMinutes(30),
            claims: authClaims,
            signingCredentials: new SigningCredentials(authSigningKey, SecurityAlgorithms.HmacSha256)
        );
    }
}
