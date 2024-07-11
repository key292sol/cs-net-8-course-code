using Dapper;
using DotnetAPI.Data;
using DotnetAPI.DTO;
using DotnetAPI.Helpers;
using DotnetAPI.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DotnetAPI.Controllers;

[Authorize]
[ApiController]
[Route("[controller]")]
public class UserCompleteController : ControllerBase
{
	private DataContextDapper dapper;

	public UserCompleteController(IConfiguration config)
	{
		// config.GetConnectionString("Default");
		dapper = new DataContextDapper(config);
	}

	[HttpGet("/GetUsers/{userId}/{isActive}")]
	public IEnumerable<UserComplete> GetUsers(int userId = 0, bool isActive = false)
	{
		string sql = "EXEC TutorialAppSchema.spUsers_Get";

		string strParameters = "";
		DynamicParameters sqlParameters = new DynamicParameters();

		if (userId != 0)
		{
			strParameters += ", @UserId=@UserIdParam";
			sqlParameters.Add("@UserIdParam", userId, System.Data.DbType.Int32);
		}
		if (isActive)
		{
			strParameters += $", @Active=1";
		}
		if (strParameters.Length > 1)
		{
			sql += strParameters[1..];
		}
		return dapper.LoadDataWithParameters<UserComplete>(sql, sqlParameters);
	}

	[HttpPut("/UpsertUser")]
	public IActionResult UpsertUser(UserComplete user)
	{
		int res = new ReusableSql(dapper).UpsertUser(user);
		if (res == 0) throw new Exception("Failed to Update User");
		return Ok();
	}

	[HttpDelete("/DeleteUser/{userId}")]
	public IActionResult DeleteUser(int userId)
	{
		string deleteSql = $"EXEC TutorialAppSchema.spUser_Delete @UserId = @UserIdParam";
		DynamicParameters sqlParameters = new DynamicParameters();
		sqlParameters.Add("@UserIdParam", userId, System.Data.DbType.Int32);

		int res = dapper.ExecuteWithParameters(deleteSql, sqlParameters);
		if (res == 0) throw new Exception("Failed to Delete User");
		return Ok();
	}
}