
namespace Trucks
{
    public interface IFuelChargeRepository
    {
        IEnumerable<FuelCharge> Charges { get; set; }
        double GetFuelCharges(int year, int week, int truckId);
        Task LoadAsync(string filename);
        void SaveAsync(IEnumerable<FuelCharge> charges);
    }
}
