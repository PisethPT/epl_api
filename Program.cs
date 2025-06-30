using epl_api.Endpoints;
using epl_api.Startup;

var builder = WebApplication.CreateBuilder(args);

builder.AddDependencies();

var app = builder.Build();

app.UseOpenApi();
app.UseAuthentications();
app.UseAuthorization();
app.UseHttpsRedirection();
app.UseStaticFiles();

// map all endpoints
app.MapsGroup();

app.UseCorsPolicy();
app.UseSessions();
app.Run();