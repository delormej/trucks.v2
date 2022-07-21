namespace Trucks
{
    public interface ISettlementRepository
    {
        Task SaveSettlementAsync(SettlementHistory entity);
        Task<IEnumerable<SettlementHistory>> GetSettlementsAsync(int companyId, int year, int week);
        Task<IEnumerable<SettlementHistory>> GetSettlementsAsync();
        
        Task SaveConvertStateAsync(ConvertState state);
        
        Task<User> GetUserAsync(string email);
        Task CreateUserAsync(User user);
        Task CreateCompanyAsync(Company company);
    }
}
