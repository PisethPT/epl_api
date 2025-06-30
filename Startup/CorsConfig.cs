namespace epl_api.Startup;

public static class CorsConfig
{
    private const string CorsPolicyName = "EPL-X-POLICY";
    private const string CLIENT_URL = "http://127.0.0.1:5500";
    //private const string CLIENT_URL = "https://vls9q40q-5500.asse.devtunnels.ms";

    public static void AddCorsPolicyServices(this IServiceCollection services)
    {
        services.AddCors(options =>
        {
            options.AddPolicy(CorsPolicyName, policy =>
            {
                policy.WithOrigins(CLIENT_URL)
                .AllowAnyHeader()
                .AllowAnyMethod();
            });
        });
    }

    public static void UseCorsPolicy(this WebApplication app)
    {
        app.UseCors(CorsPolicyName);
    }
}
