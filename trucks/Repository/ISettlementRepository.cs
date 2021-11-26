namespace Trucks
{
    public interface ISettlementRepository
    {
        Task SaveSettlementAsync(SettlementHistory entity);
        List<SettlementHistory> GetSettlements(int year, int[] weeks);
        List<SettlementHistory> GetSettlements();
        
        Task SaveConvertStateAsync(ConvertState state);
        
        Task<User> GetUserAsync(string email);
        Task CreateUserAsync(User user);
    }
}
