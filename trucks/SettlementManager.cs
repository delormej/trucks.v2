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
        ExcelConverter _converter;

        public SettlementManager(IConfiguration config, ILogger<SettlementManager> log)
        {
            _config = config;
            _converter = new ExcelConverter(_config["ZamzarKey"]);
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

            await foreach(var download in panther.DownloadSettlementsAsync(
                (s => s.SettlementDate >= watermark)) )
            {
                string filename = download.Key;
                var result = await _converter.UploadAsync(filename);

                var conversion = new ConvertState(download.Value, result.id, 
                        filename, DateTime.UtcNow);
                
                _conversionQueue.Add(conversion, SaveConvertedAsync);
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

            }
            // Also check that it's not errored.
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