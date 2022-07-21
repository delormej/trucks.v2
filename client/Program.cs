using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Trucks;

ImportSettlements(args[0]);

return;

EventWaitHandle ewh = new EventWaitHandle(false, EventResetMode.ManualReset);
EventHandler Reset = (o, e) => {
    Console.WriteLine("Signaling stop.");
    ewh.Set();
};

IConfiguration config = GetConfiguration();

Console.WriteLine("Press any key...");
Console.ReadKey();
System.Console.WriteLine("Running...");

ISettlementRepository repository = new FirestoreRepository(config, 
    CreateLogger<FirestoreRepository>());
IFileRepository file = new CloudFileRepository();

await ExportSettlements("./settlements.json");
return;

var manager = new SettlementManager(repository, file, Reset,
    GetConfiguration(), 
    CreateLogger<SettlementManager>());

// try
// {
//     var driverSettlements = await manager.GetDriverSettlementsAsync();
//     foreach (var d in driverSettlements)
//     {
//         Console.WriteLine($"{d.Driver}, {d.SettlementDate}, {d.Year}/{d.Week}, {d.Credits.Sum(c => c.ExtendedAmount)}, {d.Deductions.Sum(d => d.Amount)}");
//     }

//     var json = System.Text.Json.JsonSerializer.Serialize<IEnumerable<DriverSettlement>>(driverSettlements); 
//     File.WriteAllText("./driversettlements.json", json);
// }
// catch (Exception e)
// {
//     Console.WriteLine(e);
// }

await manager.ConvertAsync("170087");
ewh.WaitOne();

// await manager.SaveAsync("170087/CD658726.xlsx", 
//     new SettlementHistory() { CompanyId = 170087, SettlementId = "CD658726" });

 ILogger<T> CreateLogger<T>()
 {
    using var loggerFactory = LoggerFactory.Create(builder =>
    {
        builder.AddConsole();
    });

    return loggerFactory.CreateLogger<T>();
 }

 IConfiguration GetConfiguration()
 {
    return new ConfigurationBuilder()
        .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
        .AddEnvironmentVariables()
        .AddCommandLine(args)
        .Build();
 }

void ImportSettlements(string jsonPath)
{
    string json = File.ReadAllText(jsonPath);
    var settlements = System.Text.Json.JsonSerializer.Deserialize<IEnumerable<SettlementHistory>>(json);

    foreach (var settlement in settlements)
    {
        System.Console.WriteLine(settlement.SettlementId);
    }
}

async Task ExportSettlements(string path)
{
    var settlements = await repository.GetSettlementsAsync();

    var json = System.Text.Json.JsonSerializer.Serialize<IEnumerable<SettlementHistory>>(settlements); 
    File.WriteAllText(path, json);
}