using System.Data;
using MySql.Data.MySqlClient;
using ProyectoFisca.Sources.Interface;

namespace ProyectoFisca.DataAccess
{
    public class DbConnectionFactory : IDbConnectionFactory
    {
        private readonly string _connectionString;

        public DbConnectionFactory(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("FiscaliaDatabase")
                ?? throw new InvalidOperationException("La cadena de conexión 'FiscaliaDatabase' no se encuentra configurada en appsettings.json.");
        }

        public IDbConnection CreateConnection()
        {
            return new MySqlConnection(_connectionString);
        }
    }
}
