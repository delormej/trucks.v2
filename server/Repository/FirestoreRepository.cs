using Google.Cloud.Firestore;

namespace Trucks
{
    public class FirestoreRepository : ISettlementRepository
    {
        private readonly ILogger<FirestoreRepository> _log;
        private readonly PublisherService _publisher;
        private readonly FirestoreDb _firestore;
        private readonly string _eventsCollection;
        private readonly string _usersCollection;

        public FirestoreRepository(IConfiguration config, ILogger<FirestoreRepository> log,
            PublisherService publisher)
        {
            _eventsCollection = config.GetSection("MongoDb")["EventsCollectionName"];
            _usersCollection = config.GetSection("MongoDb")["UsersCollectionName"];
            _log = log;
            _publisher = publisher;

            _firestore = new FirestoreDbBuilder
            {
                ProjectId = config["ProjectId"],
                ConverterRegistry = new ConverterRegistry
                {
                    new GenericFirestoreConverter<User>("Email")
                }
            }.Build();            
        }

        public Task SaveSettlementAsync(SettlementHistory entity) {throw new NotImplementedException(); }
        public Task SaveSettlementsAsync(IEnumerable<SettlementHistory> settlements) {throw new NotImplementedException(); }
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