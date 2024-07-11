using System.Data;
using Dapper;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;

namespace DbConn.Data
{
	public class DataContextDapper
	{
		// private string connectionString = "Server=localhost;Database=DotNetCourseDatabase;Trusted_Connection=true;TrustServerCertificate=True;";
		IDbConnection dbConnection;

		public DataContextDapper(IConfiguration config)
		{
			dbConnection = new SqlConnection(config.GetConnectionString("Default"));
		}

		public T LoadDataSingle<T>(string sql)
		{
			return dbConnection.QuerySingle<T>(sql);
		}

		public IEnumerable<T> LoadData<T>(string sql)
		{
			return dbConnection.Query<T>(sql);
		}

		public int ExecuteSql(string sql)
		{
			return dbConnection.Execute(sql);
		}
	}
}