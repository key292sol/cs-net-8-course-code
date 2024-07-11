using System.Data;
using Dapper;
using Microsoft.Data.SqlClient;

namespace DotnetAPI.Data
{
	public class DataContextDapper
	{
		private readonly IDbConnection dbConn;

		public DataContextDapper(IConfiguration cfg)
		{
			dbConn = new SqlConnection(cfg.GetConnectionString("Default"));
		}

		public IEnumerable<T> LoadData<T>(string sql)
		{
			return dbConn.Query<T>(sql);
		}

		public T LoadDataSingle<T>(string sql)
		{
			return dbConn.QuerySingle<T>(sql);
		}

		public IEnumerable<T> LoadDataWithParameters<T>(string sql, DynamicParameters parameters)
		{
			return dbConn.Query<T>(sql, parameters);
		}

		public T LoadDataSingleWithParameters<T>(string sql, DynamicParameters parameters)
		{
			return dbConn.QuerySingle<T>(sql, parameters);
		}

		public int ExecuteQuery(string sql)
		{
			return dbConn.Execute(sql);
		}

		public int ExecuteWithParameters(string sql, DynamicParameters parameters)
		{
			return dbConn.Execute(sql, parameters);
			// SqlCommand command = new SqlCommand(sql);
			// foreach (SqlParameter param in parameters)
			// {
			// 	command.Parameters.Add(param);
			// }

			// dbConn.Open();

			// command.Connection = (SqlConnection)dbConn;

			// int res = command.ExecuteNonQuery();
			// return res;
		}

		~DataContextDapper()
		{
			dbConn.Close();
		}
	}
}