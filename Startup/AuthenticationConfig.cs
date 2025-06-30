namespace epl_api.Startup;

public static class AuthenticationConfig
{
    public static void AddAuthenticationServices(this IServiceCollection services)
    {
        services.AddAuthorization(options =>
        {
            options.AddPolicy("OnlyAdmin", policy =>
            {
                policy.RequireRole("Admin");
            });
            options.AddPolicy("ForGuest", policy =>
            {
                policy.RequireRole("Guest");
            });
        });
    }

    public static void UseAuthentications(this WebApplication app)
    {
        app.UseAuthentication();
    }
}
