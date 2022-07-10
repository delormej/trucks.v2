using Microsoft.AspNetCore.Mvc;
using Trucks;

namespace Trucks.Server
{
    [Route("[controller]")]
    public class DriverSettlementsController : Controller
    {
        private readonly ISettlementRepository _settlementRepository;

        public DriverSettlementsController(ISettlementRepository settlementRepository)
        {
            _settlementRepository = settlementRepository;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<DriverSettlement>>> Get(
            int year, int week, int companyId)
        {
            var settlements = await GetDriverSettlementsAsync(year, week, companyId);

            if (settlements == null)
                return NotFound();

            return Ok(settlements);
        }       

        private async Task<IEnumerable<DriverSettlement>> GetDriverSettlementsAsync(
            int year, int week, int companyId)
        {
            var settlements = await _settlementRepository.GetSettlementsAsync();
            var settlement = settlements
                .Where(s => s.CompanyId == companyId && s.Year == year && s.WeekNumber == week)
                .FirstOrDefault();

            if (settlement == null)
                return null;

            var factory = new DriverSettlementFactory();
            var driverSettlements = factory.Create(settlement);

            return driverSettlements;
        }        
    }
}