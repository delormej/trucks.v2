using Google.Cloud.Firestore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;

namespace Trucks
{
    public class FirestoreRepository : ISettlementRepository
    {
        private readonly ILogger<FirestoreRepository> _log;
        private readonly FirestoreDb _firestore;
        private readonly string _settlementsCollection;
        private readonly string _usersCollection;
        const string _stateCollection = "ConvertState";

        public FirestoreRepository(IConfiguration config, ILogger<FirestoreRepository> log)
        {
            _settlementsCollection = config["SettlementCollection"];

            _log = log;

            _firestore = new FirestoreDbBuilder
            {
                ProjectId = config["ProjectId"],
                ConverterRegistry = new ConverterRegistry
                {
                    new GenericFirestoreConverter<SettlementHistory>("SettlementId"),
                    new GenericFirestoreConverter<Credit>(),
                    new GenericFirestoreConverter<Deduction>(),
                    new GenericFirestoreConverter<User>("Email"),
                    new GenericFirestoreConverter<ConvertState>("ConversionJobId")
                }
            }.Build();            
        }

        public async Task SaveSettlementAsync(SettlementHistory settlement) 
        {
            string partitionKey = settlement.CompanyId.ToString();

            var parition = _firestore.Collection(_settlementsCollection)
                .Document(partitionKey);
            
            await parition.Collection(_settlementsCollection)
                .Document(settlement.SettlementId)
                .SetAsync(settlement);
        }

        public async Task SaveConvertStateAsync(ConvertState state)
        {
            await _firestore.Collection(_stateCollection)
                .Document(state.ConversionJobId.ToString())
                .SetAsync(state);
        }

        public async Task<DateTime> GetLatestSettlementDate(string companyId)
        {
            string partitionKey = companyId;

            var parition = _firestore.Collection(_settlementsCollection)
                .Document(partitionKey);

            var query = parition.Collection(_settlementsCollection)
                .OrderByDescending("SettlementDate")
                .Limit(1);
            
            QuerySnapshot querySnapshot = await query.GetSnapshotAsync();
            var snapshot = querySnapshot.FirstOrDefault();
            
            var settlement = snapshot.ConvertTo<SettlementHistory>();

            return settlement.SettlementDate;
        }

        public async Task<DateTime> GetOldestSettlementDate(string companyId)
        {
            string partitionKey = companyId;

            var parition = _firestore.Collection(_settlementsCollection)
                .Document(partitionKey);

            var query = parition.Collection(_settlementsCollection)
                .Limit(1);
            
            QuerySnapshot querySnapshot = await query.GetSnapshotAsync();
            var snapshot = querySnapshot.FirstOrDefault();
            
            var settlement = snapshot.ConvertTo<SettlementHistory>();

            return settlement.SettlementDate;
        }


        public List<SettlementHistory> GetSettlements(int year, int[] weeks) {throw new NotImplementedException(); }
        public List<SettlementHistory> GetSettlements() {throw new NotImplementedException(); }

        public async Task<User> GetUserAsync(string id)
        {
            var doc = _firestore.Collection(_usersCollection)
                .Document(id);
            var snapshot = await doc.GetSnapshotAsync();

            if (snapshot != null)
                return snapshot.ConvertTo<User>();
            else
                return null;
        }

        public async Task CreateUserAsync(User user)
        {
            await _firestore.Collection(_usersCollection)
                .Document(user.Email)
                .SetAsync(user);
        }        
    }
}