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
        IConversionQueue _conversionQueue;

        public SettlementManager(IConfiguration config, ILogger<SettlementManager> log)
        {
            _config = config;
        }

        /// <summary>
        /// Batch operation to download xls settlements from panther, upload to
        /// service for async conversion to xlsx.
        /// </summary>
        public async Task ConvertAsync(string companyId)
        {
            // TODO: Just grab last 2 months for right now.
            DateTime watermark = DateTime.Now.AddMonths(-2);

            PantherClient panther = CreatePantherClient(companyId);
            ExcelConverter converter = new ExcelConverter(_config["ZamzarKey"]);

            await foreach(var download in panther.DownloadSettlementsAsync(
                (s => s.SettlementDate >= watermark)) )
            {
                string filename = download.Key;
                var result = await converter.UploadAsync(filename);

                var conversion = new ConvertState(download.Value, result.id, 
                        filename, DateTime.UtcNow);
                
                _conversionQueue.Add(conversion, SaveConvertedAsync);
            }
        }

        /// <summary>
        /// Downloads xlsx from conversion service, parses to business objects and
        /// persists to repository.
        /// </summary>
        /// <returns>
        /// True if succesful, False if conversion job is not complete.  All error
        /// conditions will be uncaught exceptions.
        /// </returns>
        public Task<bool> SaveConvertedAsync(ConvertState conversion)
        {
            throw new NotImplementedException();
        }
        

        private PantherClient CreatePantherClient(string companyId)
        {
            var config = _config.GetSection(PantherSettings.Section)
                .Get<PantherSettings>();

            var company = config.Companies.FirstOrDefault(c => c.CompanyId == companyId);
            
            return new PantherClient(company.User, company.Password);
        }
    }
}