using System.Data;
using System.Data.SqlClient;
using System.Threading.Tasks;

namespace MiniBiggy.Storage.SqlServer {
    public class SqlServerStorage : IDataStore {
        public string ConnectionString { get; }
        public string TableFullName { get; set; }
        public int VersionsToKeep { get; }

        public SqlServerStorage(string connectionString, string tableFullName, int versionsToKeep = 100) {
            ConnectionString = connectionString;
            TableFullName = tableFullName;
            VersionsToKeep = versionsToKeep;
            EnsureTables();
        }

        public void EnsureTables() {
            var commandText = $@"
                    IF (NOT EXISTS (SELECT * 
                                FROM INFORMATION_SCHEMA.TABLES 
                                where  TABLE_NAME = '{TableFullName}'))
                    BEGIN
                        CREATE TABLE {TableFullName} (
	                        version int NOT NULL IDENTITY (1, 1),
	                        datetime datetime NOT NULL DEFAULT GETDATE(),
	                        data varbinary(MAX) NOT NULL,
	                        CONSTRAINT [PK_{TableFullName}] PRIMARY KEY CLUSTERED (version)
                        )
                    END
                ";
            using (var conn = new SqlConnection(ConnectionString)) {
                conn.Open();
                using (var command = new SqlCommand(commandText, conn)) {
                    command.ExecuteNonQuery();
                }
            }
        }

        public Task<byte[]> ReadAllAsync() {
            var commandText = $@"
                select data from {TableFullName}
                where version = (select max(version) from {TableFullName})
            ";
            using (var conn = new SqlConnection(ConnectionString)) {
                conn.Open();
                using (var command = new SqlCommand(commandText, conn)) {
                    return Task.FromResult((byte[])(command.ExecuteScalar() ?? new byte[0]));
                }
            }
        }

        public Task WriteAllAsync(byte[] list) {
            var commandText = $@"
                insert into {TableFullName} (data) values (@data)
            ";
            using (var conn = new SqlConnection(ConnectionString)) {
                conn.Open();
                using (var cmd = new SqlCommand(commandText, conn)) {
                    cmd.Parameters.Add("@data", SqlDbType.VarBinary).Value = list;
                    cmd.ExecuteNonQuery();
                }
                DeleteExtraEntries(conn);
            }
            return Task.CompletedTask;
        }

        private void DeleteExtraEntries(SqlConnection conn) {
            var commandText = $@"
                delete from {TableFullName} where version <= (select top 1 version - {VersionsToKeep} from {TableFullName} order by version desc)
            ";
            using (var cmd = new SqlCommand(commandText, conn)) {
                cmd.ExecuteNonQuery();
            }
        }
    }

    public static class CreateListExtensions {
        public static IChooseSerializer<T> SavingOnSqlServer<T>(this CreateList<T> createList) where T : new() {
            return createList;
        }
    }
}
