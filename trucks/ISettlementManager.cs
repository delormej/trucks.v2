namespace Trucks
{
    public interface ISettlementManager
    {
        Task ConvertAsync(string companyId);
        Task SaveConvertedAsync(ConvertState conversion);
    }
}