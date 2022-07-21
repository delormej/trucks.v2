using Google.Cloud.Firestore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using GcpHelpers.Firestore;

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
                    new GenericFirestoreConverter<User>("Email"),
                    new GenericFirestoreConverter<Credit>(),
                    new GenericFirestoreConverter<Deduction>(),
                    new GenericFirestoreConverter<ConvertState>("ConversionJobId"),
                    new GenericFirestoreConverter<Company>()
                }
            }.Build();            
        }

        /// <summary>
        /// Creates a top level company object that Settlements will live under.
        /// If this entity is not created, the top level collection will not be 
        /// queryable.  https://stackoverflow.com/questions/48498342/firestore-query-documents-with-only-collections-inside
        /// </summary>
        public async Task CreateCompanyAsync(Company company)
        {
            await _firestore.Collection(_settlementsCollection)
                .Document(company.CompanyId)
                .SetAsync(company);
        }

        public async Task SaveSettlementAsync(SettlementHistory settlement) 
        {
            string partitionKey = settlement.CompanyId.ToString();

            var parition =  _firestore.Collection(_settlementsCollection)
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

            var settlement = snapshot?.ConvertTo<SettlementHistory>();

            if (settlement != null)
                return settlement.SettlementDate;
            else 
                return DateTime.MinValue;
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
            
            var settlement = snapshot?.ConvertTo<SettlementHistory>();

            if (settlement != null)
                return settlement.SettlementDate;
            else 
                return DateTime.MinValue;
        }

        public async Task<IEnumerable<SettlementHistory>> GetSettlementsAsync(int companyId, int year, int week) 
        {
            /* Google Search doesn't yield good results, but this shows C# example:
                https://cloud.google.com/firestore/docs/query-data/queries
            */

            string partitionKey = companyId.ToString();

            var partition = _firestore.Collection(_settlementsCollection)
                .Document(partitionKey);

            var query = partition.Collection(_settlementsCollection)
                .WhereEqualTo("WeekNumber", week)
                .WhereEqualTo("Year", year);
            
            QuerySnapshot querySnapshot = await query.GetSnapshotAsync();
            
            var settlements = querySnapshot.Documents
                .Select(d => d.ConvertTo<SettlementHistory>());

            return settlements;
        }
        
        public async Task<IEnumerable<SettlementHistory>> GetSettlementsAsync() 
        {
            var settlements = new List<SettlementHistory>();

            var companiesRef = _firestore.Collection(_settlementsCollection);
            var companiesSnapshot = await companiesRef.GetSnapshotAsync();
            
            foreach (var company in companiesSnapshot.Documents)
            {
                string partitionKey = company.Id;

                var parition = _firestore.Collection(_settlementsCollection)
                    .Document(partitionKey);

                var query = parition.Collection(_settlementsCollection);
                var querySnapshot = await query.GetSnapshotAsync();
                
                foreach (var snapshot in querySnapshot.Documents)
                {
                    var settlement = snapshot.ConvertTo<SettlementHistory>();
                    settlements.Add(settlement);
                }
            }

            return settlements;
        }

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