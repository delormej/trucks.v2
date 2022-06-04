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
        ConversionJobQueue _conversionQueue;
        ExcelConverter _converter;
        ISettlementRepository _settlementRepository;
        IFileRepository _file;

        public SettlementManager(ISettlementRepository repository, IFileRepository file,
                EventHandler onFinished,
                IConfiguration config, 
                ILogger<SettlementManager> log)
        {
            _config = config;
            _converter = new ExcelConverter(_config["ZamzarKey"]);
            _settlementRepository = repository;
            _file = file;

            _conversionQueue = new ConversionJobQueue(TryConversionJobAsync);
            _conversionQueue.OnFinished += onFinished;
        }

        /// <summary>
        /// Batch operation to:
        ///     1. Download xls settlements from panther
        ///     2. Upload to service for async conversion to xlsx.
        ///     3. Persist raw xls for future processing / errors. 
        /// </summary>
        public async Task ConvertAsync(string companyId)
        {
            var watermark = await GetWatermarkAsync(companyId);

            PantherClient panther = CreatePantherClient(companyId);

            await foreach(var download in panther.DownloadSettlementsAsync(
                watermark.Filter(), 4))
            {
                string filename = download.Key;
                var result = await _converter.UploadAsync(filename);
                
                var cloudPath = await _file.SaveAsync(filename);

                Console.WriteLine($"Uploaded {filename}");

                var conversion = new ConvertState{
                    ConversionJobId = result.id,
                    Settlement = download.Value,
                    LocalXlsPath = filename,
                    CloudPath = cloudPath,
                    UploadTimestampUtc = DateTime.UtcNow
                };
                
                _conversionQueue.Add(conversion);
            }
        }

        /// <summary>
        /// Downloads xlsx from conversion service, parses to business objects and
        /// persists to repository.  If conversion is not complete yet, adds back
        /// to the queue.
        /// </summary>
        public async Task TryConversionJobAsync(ConvertState state)
        {
            var result = await _converter.QueryAsync(state.ConversionJobId);
            
            if (result.Success)
            {
                await ProcessConversionJobAsync(state, result);
            }
            else if (!result.Failed)
            {
                // Also check that it's not errored, this could be infinite loop!
                _conversionQueue.Add(state);
            }

            await _converter.DeleteAsync(result.target_files[0].id);
        }

        public async Task<IEnumerable<DriverSettlement>> GetDriverSettlementsAsync()
        {
            var settlements = await _settlementRepository.GetSettlementsAsync();
            var settlement = settlements.First();

            var factory = new DriverSettlementFactory();
            var driverSettlements = factory.Create(settlement);

            return driverSettlements;
        }

        private async Task ProcessConversionJobAsync(ConvertState state, ZamzarResult result)
        {
            string filename = Path.Combine(state.Settlement.CompanyId.ToString(), 
                result.target_files[0].name);
            
            int fileId = result.target_files[0].id;
            
            if (await _converter.DownloadAsync(fileId, filename))
            {
                state.ConvertTimestampUtc = DateTime.Parse(result.finished_at);                    
                state.CloudPath = await _file.SaveAsync(filename); 

                await SaveSettlementAsync(filename, state.Settlement);
                
                await _settlementRepository.SaveConvertStateAsync(state);
            }            
        }

        public async Task SaveSettlementAsync(string filename, SettlementHistory settlement)
        {
            SettlementHistory parsedSettlement = SettlementHistoryParser.Parse(filename);
            if (parsedSettlement != null)
            {
                await _file.SaveAsync(filename);

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

        private PantherClient CreatePantherClient(string companyId)
        {
            var config = _config.GetSection(PantherSettings.Section)
                .Get<PantherSettings>();

            var company = config.Companies.FirstOrDefault(
                c => c.CompanyId == companyId);
            
            return new PantherClient(company.User, company.Password);
        }

        // public async Task CreateWorkbooks() 
        // {
        //     SettlementWorkbookGenerator generator = new SettlementWorkbookGenerator(settlements, fuelRepository);
        //     foreach (string driver in GetDrivers(settlements, truckid))
        //     {
        //         string file = generator.Generate(year, weeks, driver);
        //         files.Add(file);
        //     }

        // } 

        private IEnumerable<string> GetDrivers(List<SettlementHistory> settlements, int? truckid)
        {
            IEnumerable<string> drivers = settlements.SelectMany(s => 
                    s.Credits.Where(c => truckid != null ? c.TruckId == truckid : true)
                    .Select(c => c.Driver)).Distinct();                
            
            return drivers;
        }        

        internal class Watermark
        {
            public DateTime low;
            public DateTime high;

            public Func<SettlementHistory, bool> Filter()
            {
                return (s => s.SettlementDate >= this.high ||
                    s.SettlementDate <= this.low);
            }
        }

        private async Task<Watermark> GetWatermarkAsync(string companyId)
        {
            // Not part of the interface right now, so cast temporarily until we 
            // decide this is the right way to do this or not.
            FirestoreRepository firestore = (FirestoreRepository)_settlementRepository;
            
            var high = await firestore.GetLatestSettlementDate(companyId);

            var low = await firestore.GetOldestSettlementDate(companyId);

            Console.WriteLine($"High: {high}, Low: {low}");
            return new Watermark() {low = low, high = high};
        }

    }
}