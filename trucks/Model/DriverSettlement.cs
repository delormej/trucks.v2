namespace Trucks
{
    public class DriverSettlement
    {
        public Guid DriverSettlementId { get; set; }
        public string SettlementId { get; set; }
        public int CompanyId { get; set; }
        public int Year { get; set; }
        public int Week { get; set; }
        public int[] Trucks { get; set; }
        public string Driver;
        public DateTime SettlementDate;
        public IEnumerable<Deduction> Deductions;
        public IEnumerable<Credit> Credits;
        public double Fuel { get; set; }
        public double OccupationalInsurance { get; set; }
        public bool IgnoreComchek;
    }
}