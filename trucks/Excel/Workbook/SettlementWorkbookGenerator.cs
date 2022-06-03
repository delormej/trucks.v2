using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Trucks.Excel
{
    public class SettlementWorkbookGenerator
    {
        private List<SettlementHistory> _settlements;
        private IFuelChargeRepository _fuelRepository;

        public SettlementWorkbookGenerator(List<SettlementHistory> settlements, 
                IFuelChargeRepository fuelRepository = null)
        {
            this._settlements = settlements;
            this._fuelRepository = fuelRepository;
        }

        public string Generate(IEnumerable<DriverSettlement> driverSettlements)
        {
            string outputFile = null;
            SettlementWorkbook workbook = null;
            
            try
            {
                foreach (var settlement in driverSettlements.OrderBy(s => s.Week))
                {
                    if (workbook == null)
                    {
                        workbook = new SettlementWorkbook(settlement.Year, 
                            settlement.Trucks.First(), // TODO: fix to deal with multiple trucks.
                            settlement.Driver);
                        outputFile = workbook.Create();
                    }
                    if (workbook == null)
                    {
                        System.Console.WriteLine($"No workbook created for driver {settlement.Driver} on week {settlement.Week}.");
                        continue;
                    }
                    
                    workbook.AddSheet(settlement.Week, settlement.SettlementDate);
                    workbook.AddSettlementId(settlement.SettlementId);

                    if (settlement.Fuel > 0)
                    {
                        workbook.AddFuelCharge(settlement.Fuel);
                        settlement.IgnoreComchek = true;
                    }

                    workbook.AddCredits(settlement.Credits, settlement.IgnoreComchek);                    

                    if (settlement.OccupationalInsurance > 0)
                        workbook.AddOccupationalInsurance(settlement.OccupationalInsurance);
                        
                    workbook.Save();

                    System.Console.WriteLine($"Created {outputFile} with {settlement.Credits.Count()} credit(s), ${settlement.Fuel.ToString("0.00")} in fuel from {settlement.SettlementId}:{settlement.SettlementDate.ToString("yyyy-MM-dd")} for company {settlement.CompanyId}.");
                }
            }
            catch (Exception e)
            {
                System.Console.WriteLine($"Error generating workbook {outputFile ?? "null"}\n\t{e.Message}");
            }
            finally
            {
                if (workbook != null)
                    workbook.Dispose();
            }

            return outputFile;
        }
    }
}