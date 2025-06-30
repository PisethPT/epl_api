namespace epl_api.Startup;

public static class SessionsConfig
{
    public static void AddSessions(this IServiceCollection services)
    {
        services.AddDistributedMemoryCache();
        services.AddSession(options =>
        {
            options.IdleTimeout = TimeSpan.FromMinutes(30);
            options.Cookie.HttpOnly = true;
            options.Cookie.IsEssential = true;
        });
    }

    public static void UseSessions(this WebApplication app)
    {
        app.UseSession();
    }
}
