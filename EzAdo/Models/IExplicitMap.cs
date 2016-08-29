using System.Data.SqlClient;

namespace EzAdo.Models
{
    public interface IExplicitMap
    {
        void Map(string procedureName, SqlDataReader rdr);
        bool SupportsMap(string procedureName);
    }
}
