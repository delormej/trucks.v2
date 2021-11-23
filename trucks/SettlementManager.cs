using Trucks.Panther;
using Trucks.Excel;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;

namespace Trucks
{    
    public class SettlementManager : ISettlementManager
    {
        readonly IConfiguration _config;
        ThreadedConversionQueue _conversionQueue;
        ExcelConverter _converter;
        ISettlementRepository _settlementRepository;

        public SettlementManager(ISettlementRepository repository, IConfiguration config, 
                ILogger<SettlementManager> log)
        {
            _config = config;
            _converter = new ExcelConverter(_config["ZamzarKey"]);
            _settlementRepository = repository;
            _conversionQueue = new ThreadedConversionQueue(SaveConvertedAsync);
            _conversionQueue.OnFinished += (o, e) => 
            {
                System.Console.WriteLine("Finished");
            };
        }

        /// <summary>
        /// Batch operation to download xls settlements from panther, upload to
        /// service for async conversion to xlsx.
        /// </summary>
        public async Task ConvertAsync(string companyId)
        {
            // var c = new ConvertState(
            //     settlement: new SettlementHistory() { CompanyId = 170087 },
            //     conversionJobId: 23533273, 
            //     xlsPath: "170087/CD658438.xls",
            //     uploadTimestampUtc: DateTime.UtcNow.AddDays(-1));
            // _conversionQueue.Add(c);
            // return;

            // TODO: Just grab last 2 months for right now.
            DateTime watermark = DateTime.Now.AddMonths(-3);

            PantherClient panther = CreatePantherClient(companyId);

            await foreach(var download in panther.DownloadSettlementsAsync(
                (s => s.SettlementDate >= watermark), 1))
            {
                string filename = download.Key;
                var result = await _converter.UploadAsync(filename);
                Console.WriteLine($"Uploaded {filename}");

                var conversion = new ConvertState(download.Value, result.id, 
                        filename, DateTime.UtcNow);
                
                _conversionQueue.Add(conversion);
            }
        }

        /// <summary>
        /// Downloads xlsx from conversion service, parses to business objects and
        /// persists to repository.  If conversion is not complete yet, adds back
        /// to the queue.
        /// </summary>
        public async Task SaveConvertedAsync(ConvertState state)
        {
            var result = await _converter.QueryAsync(state.conversionJobId);
            
            if (result.Success)
            {
                string filename = Path.Combine(state.settlement.CompanyId.ToString(), 
                    result.target_files[0].name);
                
                int fileId = result.target_files[0].id;
                
                if (await _converter.DownloadAsync(fileId, filename))
                {
                    System.Console.WriteLine($"Downloaded: {filename}");
                    await SaveAsync(filename, state.settlement);
                }
            }
            else if (!result.Failed)
            {
                // Also check that it's not errored, this could be infinite loop!
                _conversionQueue.Add(state);
            }

            await _converter.DeleteAsync(result.target_files[0].id);
        }
        
        private PantherClient CreatePantherClient(string companyId)
        {
            var config = _config.GetSection(PantherSettings.Section)
                .Get<PantherSettings>();

            var company = config.Companies.FirstOrDefault(
                c => c.CompanyId == companyId);
            
            return new PantherClient(company.User, company.Password);
        }

        private async Task SaveAsync(string filename, SettlementHistory settlement)
        {
            SettlementHistory parsedSettlement = SettlementHistoryParser.Parse(filename);
            if (parsedSettlement != null)
            {
                settlement.Credits = parsedSettlement.Credits;
                settlement.Deductions = parsedSettlement.Deductions;

                await _settlementRepository.SaveSettlementAsync(settlement);
                
                Console.WriteLine($"Saved {settlement.SettlementId} to db.");   
            }
            else
            {
                Console.WriteLine($"Unable to parse {filename}.");
            }
        }
    }
}