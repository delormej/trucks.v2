namespace Trucks
{
    public class ConsoleRepository : ISettlementRepository
    {
        public Task SaveSettlementAsync(SettlementHistory settlement)
        {
            System.Console.WriteLine(settlement);
            System.Console.WriteLine($"{settlement.SettlementDate}, {settlement.CheckAmount}");
            return Task.CompletedTask;
        }

        public Task SaveConvertStateAsync(ConvertState state)
        {throw new NotImplementedException();}
        
        public List<SettlementHistory> GetSettlements(int year, int[] weeks)
        {throw new NotImplementedException();}
        
        public List<SettlementHistory> GetSettlements()
        {throw new NotImplementedException();}

        public Task<User> GetUserAsync(string email)
        {throw new NotImplementedException();}
        public Task CreateUserAsync(User user)
        {throw new NotImplementedException();}
    }
}