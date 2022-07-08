using Microsoft.AspNetCore.Mvc;
using Trucks;

namespace Trucks.Server
{
    [Route("[controller]")]
    public class SettlementsController : Controller
    {
        private readonly ISettlementRepository _settlementRepository;

        public SettlementsController(ISettlementRepository settlementRepository)
        {
            _settlementRepository = settlementRepository;
        }

        [HttpGet]
        public async Task<IEnumerable<DriverSettlement>> Get()
        {
            var settlements = await GetDriverSettlementsAsync();
            return settlements;
        }       

        private async Task<IEnumerable<DriverSettlement>> GetDriverSettlementsAsync()
        {
            var settlements = await _settlementRepository.GetSettlementsAsync();
            var settlement = settlements.First();

            var factory = new DriverSettlementFactory();
            var driverSettlements = factory.Create(settlement);

            return driverSettlements;
        }        
    }
}