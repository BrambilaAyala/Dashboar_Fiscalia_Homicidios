using System.Data;

namespace ProyectoFisca.Sources.Interface
{
    public interface IDbConnectionFactory
    {
        IDbConnection CreateConnection();
    }
}
