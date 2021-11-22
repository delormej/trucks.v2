namespace Trucks
{
    public interface ISettlementManager
    {
        Task ConvertAsync(string companyId);
        Task<bool> SaveConvertedAsync(ConvertState conversion);
    }

    public record ConvertState 
    (
        SettlementHistory settlement,
        int conversionJobId,
        string xlsPath,
        DateTime UploadTimestamp
    );    
}