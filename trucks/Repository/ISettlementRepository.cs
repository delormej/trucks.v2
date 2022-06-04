namespace Trucks
{
    public interface ISettlementRepository
    {
        Task SaveSettlementAsync(SettlementHistory entity);
        IEnumerable<SettlementHistory> GetSettlements(int year, int[] weeks);
        Task<IEnumerable<SettlementHistory>> GetSettlementsAsync();
        
        Task SaveConvertStateAsync(ConvertState state);
        
        Task<User> GetUserAsync(string email);
        Task CreateUserAsync(User user);
    }
}
