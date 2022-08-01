using Microsoft.AspNetCore.Mvc;
using static Trucks.Tools;

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
        public async Task<ActionResult<IEnumerable<SettlementHistory>>> Get(
            string companyId, int? year, int? week)
        {
            IEnumerable<SettlementHistory> settlements = null;

            if (companyId == null && year == null && week == null)
            {
                settlements = await _settlementRepository.GetSettlementsAsync();
            }
            else if (companyId != null)
            {
                year ??= DateTime.Now.Year;
                week ??= GetLastWeek();

                settlements = await _settlementRepository.GetSettlementsAsync(companyId, 
                    (int)year, (int)week);
            }

            if (settlements == null)
                return NotFound();

            return Ok(settlements);
        }

        [HttpGet("{companyId}/{settlementId}")]
        public async Task<ActionResult<SettlementHistory>> GetById(
            string companyId, string settlementId)
        {
            var settlement = await _settlementRepository.GetSettlementAsync(
                companyId, settlementId);

            if (settlement == null)
                return NotFound();

            return Ok(settlement);
        }


        [HttpGet("summaries")]
        public async Task<ActionResult<IEnumerable<SettlementSummary>>> GetSummaries()
        {
            var summaries = await _settlementRepository.GetSettlementSummariesAsync();

            if (summaries == null)
                return NotFound();

            return Ok(summaries);
        }
    }
}