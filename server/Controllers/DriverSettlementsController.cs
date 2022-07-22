using Microsoft.AspNetCore.Mvc;
using static Trucks.Tools;

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
            int companyId, int? year, int? week)
        {
            year ??= DateTime.Now.Year;
            week ??= GetLastWeek();

            var settlements = await GetDriverSettlementsAsync(companyId, (int)year, (int)week);
        
            if (settlements == null)
                return NotFound();

            return Ok(settlements);
        }       

        private async Task<IEnumerable<DriverSettlement>> GetDriverSettlementsAsync(
            int companyId, int year, int week)
        {
            var factory = new DriverSettlementFactory();
            var settlements = await _settlementRepository.GetSettlementsAsync(companyId, year, week);

            if (settlements == null || settlements.Count() == 0)
                return null;

            var driverSettlements = new List<DriverSettlement>();

            foreach(var settlement in settlements)
                driverSettlements.AddRange(factory.Create(settlement));
            
            return driverSettlements;
        }        
    }
}