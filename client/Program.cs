using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Trucks;

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

var manager = new SettlementManager(repository, file, Reset,
    GetConfiguration(), 
    CreateLogger<SettlementManager>());

try
{
    var driverSettlements = await manager.GetDriverSettlementsAsync();
    foreach (var d in driverSettlements)
    {
        Console.WriteLine($"{d.Driver}, {d.SettlementDate}, {d.Year}/{d.Week}, {d.Credits.Sum(c => c.ExtendedAmount)}, {d.Deductions.Sum(d => d.Amount)}");
    }
}
catch (Exception e)
{
    Console.WriteLine(e);
}

//await manager.ConvertAsync("170087");
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