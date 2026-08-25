using MySqlConnector;

namespace Business.Infrastructure.Persistence;

public sealed class MySqlConnectionFactory(string connectionString)
{
    public MySqlConnection CreateConnection() => new(connectionString);
}
