using System.Data.Common;
using System.Threading.Tasks;

namespace vcdb
{
    public class DatabaseRepository : IDatabaseRepository
    {
        private readonly ITableRepository tableRepository;

        public DatabaseRepository(ITableRepository tableRepository)
        {
            this.tableRepository = tableRepository;
        }

        public async Task<DatabaseDetails> GetDatabaseDetails(DbConnection connection)
        {
            return new DatabaseDetails
            {
                Tables = await tableRepository.GetTables(connection)
            };
        }
    }
}
