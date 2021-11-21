namespace Trucks.Server
{
    public interface ISettlementRepository
    {
        Task SaveSettlementAsync(SettlementHistory entity);
        Task SaveSettlementsAsync(IEnumerable<SettlementHistory> settlements);
        List<SettlementHistory> GetSettlements(int year, int[] weeks);
        List<SettlementHistory> GetSettlements();

        Task<User> GetUserAsync(string email);
        Task CreateUserAsync(User user);
    }
}
