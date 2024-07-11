using DotnetAPI.Models;

namespace DotnetAPI.Data
{
	public class UserRepository : IUserRepository
	{
		private DataContextEF ef;

		public UserRepository(IConfiguration config)
		{
			// config.GetConnectionString("Default");
			ef = new DataContextEF(config);
		}

		public bool SaveChanges()
		{
			return ef.SaveChanges() > 0;
		}

		public bool AddEntity<T>(T entityToAdd)
		{
			if (entityToAdd != null)
			{
				ef.Add(entityToAdd);
				return true;
			}
			return false;
		}

		public bool RemoveEntity<T>(T entityToRemove)
		{
			if (entityToRemove != null)
			{
				ef.Remove(entityToRemove);
				return true;
			}
			return false;
		}

		public IEnumerable<User> GetUsers()
		{
			return ef.Users.ToList<User>();
		}

		public User? GetSingleUser(int userId)
		{
			return ef.Users.Where<User>(u => u.UserId == userId).FirstOrDefault<User>();
		}
	}
}