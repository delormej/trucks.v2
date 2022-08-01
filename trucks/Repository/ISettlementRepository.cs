namespace Trucks
{
    public interface ISettlementRepository
    {
        Task SaveSettlementAsync(SettlementHistory entity);
        Task<SettlementHistory> GetSettlementAsync(string companyId, string settlementId);
        Task<IEnumerable<SettlementHistory>> GetSettlementsAsync(string companyId, int year, int week);
        Task<IEnumerable<SettlementHistory>> GetSettlementsAsync();
        Task<IEnumerable<SettlementSummary>> GetSettlementSummariesAsync();
        Task SaveConvertStateAsync(ConvertState state);
        
        Task<User> GetUserAsync(string email);
        Task CreateUserAsync(User user);
        Task CreateCompanyAsync(Company company);
    }
}
