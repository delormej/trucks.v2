namespace Trucks
{
    public class DriverSettlementFactory
    {
        private IFuelChargeRepository _fuelRepository;

        public DriverSettlementFactory(IFuelChargeRepository fuel = null)
        {
            _fuelRepository = fuel;
        }

        public IEnumerable<DriverSettlement> Create(SettlementHistory settlement)
        {
            var driverSettlements = new List<DriverSettlement>();

            var drivers = settlement.Credits.GroupBy(c => c.Driver)
                .Select(driver => driver.Key);

            foreach (var driver in drivers)
                driverSettlements.Add(Create(settlement, driver));

            return driverSettlements;
        }

        public DriverSettlement Create(SettlementHistory settlement, string driver)
        {
            int[] trucks = GetTrucksForDriver(settlement, driver);

            var driverSettlement = new DriverSettlement() 
            {
                DriverSettlementId = Guid.NewGuid(),
                CompanyId = settlement.CompanyId,
                SettlementId = settlement.SettlementId,
                SettlementDate = settlement.SettlementDate,
                Year = settlement.Year,
                Week = settlement.WeekNumber,
                Driver = driver,
                Trucks = trucks,
                Deductions = settlement.Deductions.Where(c => trucks.Contains(c.TruckId)),
                Credits = settlement.Credits.Where(c => trucks.Contains(c.TruckId)),
                Fuel = GetFuel(settlement.Year, settlement.WeekNumber, trucks)
            };
           
            driverSettlement.OccupationalInsurance = 
                GetOccupationalInsurance(driverSettlement.Deductions);

            #warning FIX THIS: Need to get Comchek flag from Driver.
            driverSettlement.IgnoreComchek = false;

            return driverSettlement;
        }

        private DateTime GetSheetSettlementDate(SettlementHistory settlement)
        {
            DateTime sheetSettlementDate = settlement.SettlementDate.AddDays(7);
            if (sheetSettlementDate.DayOfWeek != DayOfWeek.Friday)
                throw new ApplicationException($"Settlement date must be a Friday: {settlement.SettlementDate}");            
            return sheetSettlementDate;
        }

        private int[] GetTrucksForDriver(SettlementHistory settlement, string driver)
        {
            var trucks = settlement.Credits.Where(c => c.Driver == driver)
                .Select(c => c.TruckId);
            
            if (trucks?.Count() <= 0)
                throw new ApplicationException($"Unable to find a truckid for driver {driver} in settlement {settlement.id}");

            return trucks.ToArray();
        }

        private double GetOccupationalInsurance(IEnumerable<Deduction> deductions)
        {
            double value = 0.0;
            var occupationalInsurance = deductions.Where(d => 
                d.Description == "OCCUPATIONAL INSURANCE").FirstOrDefault();
            if (occupationalInsurance != null)
                value = occupationalInsurance.Amount;

            return value;
        }

        private double GetFuel(int year, int week, int[] trucks)
        {
            double fuel = 0;

            if (_fuelRepository != null)
            {
                foreach (var truck in trucks)
                    fuel += _fuelRepository.GetFuelCharges(year, week, truck);
            }

            return fuel;
        }
            

        // Externalize business logic into a predicate?
        // public Func<SettlementHistory, bool> DeductionPredicate { get; set; }

            // if company is 44510 and truck is NOT Andrew Rowan, then don't include 
            // COMCHEK PRO ADVANCE

            // if (s.CompanyId == 44510)
            // {
            //     s.Deductions.Where(d => d.TruckId != 13357);
            // }

            // ADVANCE FEE PLAN F gets added if ??

    }
}