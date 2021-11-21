using Trucks;
using Trucks.Authentication;
using Google.Cloud.Logging.Console;

var builder = WebApplication.CreateBuilder(args);

if (builder.Environment.IsProduction()) 
{
    await GoogleMetadata.SetConfigAsync(builder.Configuration);
    builder.Logging.AddGoogleFormatLogger();
}

// Services
builder.Services.AddSignalR();
builder.Services.AddHostedService<SubscriberService>();
builder.Services.AddSingleton<PublisherService>();
builder.Services.AddSingleton<ISettlementRepository, FirestoreRepository>();

// Controllers
builder.Services.AddControllers();

// AuthN/AuthZ
builder.Services.AddGoogleLoginJwt();

var app = builder.Build();

app.MapControllers();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();
app.UseDefaultFiles();
app.UseStaticFiles();

// signalR endpoint
app.UseEndpoints(endpoints =>
    endpoints.MapHub<NotifyHub>("/notifyhub")
);

try 
{
    app.Run();
}
catch (Exception e)
{
    app.Logger.LogCritical(e, "Unhandled exception");
}