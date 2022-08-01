namespace Trucks
{
    public class DriverSettlement
    {
        public Guid DriverSettlementId { get; set; }
        public string SettlementId { get; set; }
        public string CompanyId { get; set; }
        public int Year { get; set; }
        public int Week { get; set; }
        public int[] Trucks { get; set; }
        public string Driver { get; set; }
        public DateTime SettlementDate { get; set; }
        public IEnumerable<Deduction> Deductions { get; set; }
        public IEnumerable<Credit> Credits { get; set; }
        public double Fuel { get; set; }
        public double OccupationalInsurance { get; set; }
        public bool IgnoreComchek { get; set; }
    }
}