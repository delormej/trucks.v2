namespace Trucks
{
    public interface ISettlementManager
    {
        Task ConvertAsync(string companyId);
        Task TryConversionJobAsync(ConvertState conversion);
    }
}