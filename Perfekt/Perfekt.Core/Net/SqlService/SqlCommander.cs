using System.Data.SqlClient;
using System.Net;

namespace Perfekt.Core.Net.SqlService
{
    public sealed class SqlCommander
    {
        private SqlConnection connection;

        public string HostnameOrIp { get; }

        public string DBName { get; }

        public NetworkCredential Credentials { get; }

        public bool EncryptConnection { get; set; }


        public SqlCommander(string hostnameOrIp, string dbName)
        {
            this.HostnameOrIp = hostnameOrIp;
            this.Credentials = new NetworkCredential();
            this.DBName = dbName;
            this.connection = new SqlConnection();
        }

        public string GetConnectionString()
        {
            var encryptConnection = this.EncryptConnection ? "yes" : "no";

            return $"Data Source={HostnameOrIp};" +
                   $"Initial Catalog={DBName};" +
                   $"User Id={Credentials.UserName};" +
                   $"Password={Credentials.Password};" +
                   $"Encrypt={encryptConnection}";
        }

        public bool TryCreateConnection(TimeSpan connectionTimeout)
        {
            try
            {
                connection = new SqlConnection(GetConnectionString());
                var taskResult = Task.Run(async () => await connection.OpenAsync());
                if (!taskResult.Wait(connectionTimeout))
                {
                    return false;
                }

                return taskResult.Status == TaskStatus.RanToCompletion &&
                       taskResult.IsCompletedSuccessfully;
            }
            catch (Exception e)
            {
                // TODO: implement debug trace info
                return false;
            }
        }

        public SqlDataReader ExecuteQuery(string queryString)
        {
            if (connection == null)
            {
                throw new NullReferenceException(nameof(connection) + " cannot be null.");
            }

            var command = new SqlCommand(queryString, connection);
            var dataReader = command.ExecuteReader();

            return dataReader;
        }
    }
}
