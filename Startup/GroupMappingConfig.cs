using epl_api.Endpoints;

namespace epl_api.Startup;

public static class GroupMappingConfig
{
    public static void MapsGroup(this WebApplication app)
    {
        // all mapping endpoints
        app.MapRoots(app.Configuration); // for register and login endpoint
        app.MapTeams();
        app.MapPlayers();
        app.MapMatches();
        app.MapNews();
        app.MapAssists();
        app.MapCards();
        app.MapGoals();
        app.MapMatchSeason();
        app.MapSeasons();
    }
}
