
namespace Trucks
{
    public record SettlementSummary
    {
        public static string[] Fields = { "SettlementId", "SettlementDate", "WeekNumber", "Year", "CompanyId", "CheckAmount" };
        public string id => SettlementId;
        public string SettlementId { get; set; }
        public DateTime SettlementDate { get; set; }
        public int WeekNumber { get; set; }
        public int Year { get; set; }
        public int CompanyId { get; set; }
        public double CheckAmount { get; set; }
    }
}