using Trucks;
using Trucks.Server;
using GcpHelpers.Authentication;
using GcpHelpers.Metadata;
using Google.Cloud.Logging.Console;

var builder = WebApplication.CreateBuilder(args);

// if (builder.Environment.IsProduction()) 
// {
//     await GoogleMetadata.SetConfigAsync(builder.Configuration);
//     builder.Logging.AddGoogleCloudConsole();
// }

// Services
// builder.Services.AddSignalR();
// builder.Services.AddHostedService<SubscriberService>();
// builder.Services.AddSingleton<PublisherService>();
builder.Services.AddSingleton<ISettlementRepository, FirestoreRepository>();

// Controllers
builder.Services.AddControllers();

// AuthN/AuthZ
// builder.Services.AddGoogleLoginJwt();

builder.Services.AddCors(o => o.AddDefaultPolicy(builder => {
    builder.AllowAnyMethod();
    builder.AllowAnyHeader();
    builder.AllowAnyOrigin();
}));

var app = builder.Build();

app.UseCors();
app.MapControllers();
app.UseRouting();
// app.UseAuthentication();
// app.UseAuthorization();
app.UseDefaultFiles();
app.UseStaticFiles();

// signalR endpoint
// app.UseEndpoints(endpoints =>
//     endpoints.MapHub<NotifyHub>("/notifyhub")
// );

try 
{
    app.Run();
}
catch (Exception e)
{
    app.Logger.LogCritical(e, "Unhandled exception");
}