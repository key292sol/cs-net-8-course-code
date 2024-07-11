using System.Runtime.CompilerServices;
using System.Security.Claims;
using Dapper;
using DotnetAPI.Data;
using DotnetAPI.DTO;
using DotnetAPI.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DotnetAPI.Controllers;

[Authorize]
[ApiController]
[Route("[controller]")]
public class PostController : ControllerBase
{
	private readonly DataContextDapper dapper;

	public PostController(IConfiguration config)
	{
		dapper = new DataContextDapper(config);
	}

	[HttpGet("/GetPosts/{userId}/{postId}/{searchParam}")]
	public IEnumerable<Post> GetPosts(int userId = 0, int postId = 0, string searchParam = "none")
	{
		string sql = "EXEC TutorialAppSchema.spPosts_Get ";
		string strParameters = "";
		DynamicParameters sqlParameters = new DynamicParameters();

		if (userId != 0)
		{
			strParameters += $", @UserId = @UserIdParam";
			sqlParameters.Add("@UserIdParam", userId, System.Data.DbType.Int32);
		}
		if (postId != 0)
		{
			strParameters += $", @PostId = @PostIdParam";
			sqlParameters.Add("@PostIdParam", postId, System.Data.DbType.Int32);
		}
		if (searchParam != "none")
		{
			strParameters += $", @SearchVal = @SearchValParam";
			sqlParameters.Add("@SearchValParam", searchParam, System.Data.DbType.String);
		}

		if (strParameters.Length > 0) sql += strParameters[1..];
		return dapper.LoadDataWithParameters<Post>(sql, sqlParameters);
	}

	[HttpGet("/GetMyPosts")]
	public IEnumerable<Post> GetMyPosts()
	{
		int userId = Int32.Parse(this.User.FindFirstValue("userId") ?? "0");
		string sql = $@"EXEC TutorialAppSchema.spPosts_Get @UserId = @UserIdParam";
		DynamicParameters sqlParameters = new DynamicParameters();
		sqlParameters.Add("@UserIdParam", userId, System.Data.DbType.Int32);
		return dapper.LoadDataWithParameters<Post>(sql, sqlParameters);
	}

	[HttpPut("/UpsertPost")]
	public IActionResult UpsertPost(Post postToAdd)
	{
		int userId = Int32.Parse(this.User.FindFirstValue("userId") ?? "0");

		string sql = $@"EXEC TutorialAppSchema.spPosts_Upsert
							@UserId = @UserIdParam,
							@PostTitle = @PostTitleParam,
							@PostContent = @PostContentParam";

		DynamicParameters sqlParameters = new DynamicParameters();
		sqlParameters.Add("@UserIdParam", userId, System.Data.DbType.Int32);
		sqlParameters.Add("@PostTitleParam", postToAdd.PostTitle, System.Data.DbType.String);
		sqlParameters.Add("@PostContentParam", postToAdd.PostContent, System.Data.DbType.String);

		if (postToAdd.PostId > 0)
		{
			sql += $", @PostId = @PostIdParam";
			sqlParameters.Add("@PostIdParam", postToAdd.PostId, System.Data.DbType.Int32);
		}

		int res = dapper.ExecuteWithParameters(sql, sqlParameters);
		if (res == 0) return StatusCode(500, "Failed to Upsert post");

		return Ok();
	}

	[HttpDelete("/DeletePost/{postId}")]
	public IActionResult DeletePost(int postId)
	{
		int userId = Int32.Parse(this.User.FindFirstValue("userId") ?? "-1");

		string sql = $"EXEC TutorialAppSchema.spPost_Delete @PostId = @PostIdParam, @UserId = @UserIdParam";

		DynamicParameters sqlParameters = new DynamicParameters();
		sqlParameters.Add("@UserIdParam", userId, System.Data.DbType.Int32);
		sqlParameters.Add("@PostIdParam", postId, System.Data.DbType.Int32);

		int res = dapper.ExecuteWithParameters(sql, sqlParameters);
		if (res == 0) return StatusCode(500, "Failed to Delete the post");

		return Ok();
	}
}