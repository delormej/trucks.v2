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

        public Task<SettlementHistory> GetSettlementAsync(string companyId, string settlementId)
        {throw new NotImplementedException();}

        public Task<IEnumerable<SettlementHistory>> GetSettlementsAsync(string companyId, int year, int week) 
        {throw new NotImplementedException();}
        
        public Task<IEnumerable<SettlementHistory>> GetSettlementsAsync()
        {throw new NotImplementedException();}

        public Task<IEnumerable<SettlementSummary>> GetSettlementSummariesAsync()
        {throw new NotImplementedException();}

        public Task<User> GetUserAsync(string email)
        {throw new NotImplementedException();}
        public Task CreateUserAsync(User user)
        {throw new NotImplementedException();}

        public Task CreateCompanyAsync(Company company)
        {throw new NotImplementedException();}
    }
}
