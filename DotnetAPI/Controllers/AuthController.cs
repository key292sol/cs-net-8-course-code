using System.Collections.Generic;
using System.Data;
using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using AutoMapper;
using Dapper;
using DotnetAPI.Data;
using DotnetAPI.DTO;
using DotnetAPI.Helpers;
using DotnetAPI.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.IdentityModel.Tokens;

namespace DotnetAPI.Controllers
{
	[Authorize]
	[ApiController]
	[Route("[controller]")]
	public class AuthController : ControllerBase
	{
		private readonly DataContextDapper dapper;
		private readonly AuthHelper authHelper;

		public AuthController(IConfiguration _config)
		{
			dapper = new DataContextDapper(_config);
			authHelper = new AuthHelper(_config);
		}

		[AllowAnonymous]
		[HttpPost("/Register")]
		public IActionResult Register(UserRegistrationDto user)
		{
			if (!user.Password.Equals(user.PasswordConfirm))
				return StatusCode(500, "Password and Confirm passowrd are not the same");

			string sql = $"EXEC TutorialAppSchema.spUsers_Get @Email = '{user.Email}'";
			IEnumerable<string> existingUsers = dapper.LoadData<string>(sql);
			if (existingUsers.Count() > 0)
				return StatusCode(500, "Email already exists -- " + user.Email);


			bool userAuthRegistered = authHelper.SetPassword(new UserLoginDto()
			{
				Email = user.Email,
				Password = user.Password
			});

			Console.WriteLine(userAuthRegistered);
			if (!userAuthRegistered)
				return StatusCode(500, "Failed to register user password");


			IMapper mapper = new Mapper(new MapperConfiguration(config =>
			{
				config.CreateMap<UserRegistrationDto, User>();
			}));

			UserComplete userComplete = mapper.Map<UserComplete>(user);
			userComplete.Active = true;

			if (new ReusableSql(dapper).UpsertUser(userComplete) == 0)
				return StatusCode(500, "Failed to Add user");

			int userId = dapper.LoadDataSingle<int>($"SELECT UserId FROM TutorialAppSchema.Users WHERE Email = '{user.Email}'");

			return Ok(new Dictionary<string, string>() { { "token", authHelper.CreateToken(userId) } });
		}

		[AllowAnonymous]
		[HttpPost("/Login")]
		public IActionResult Login(UserLoginDto user)
		{
			string hashSaltSql = $@"EXEC TutorialAppSchema.spLoginConfirmation_Get @Email='{user.Email}'";

			// DynamicParameters sqlParameters = new DynamicParameters();
			// sqlParameters.Add("@EmailParam", user.Email, DbType.String);

			// UserLoginConfirmationDto userLoginConfirmation = dapper.LoadDataSingleWithParameters<UserLoginConfirmationDto>(hashSaltSql, sqlParameters);
			UserLoginConfirmationDto userLoginConfirmation = dapper.LoadDataSingle<UserLoginConfirmationDto>(hashSaltSql);

			byte[] passHash = authHelper.GetPasswordHash(user.Password, userLoginConfirmation.PasswordSalt);

			if (!passHash.SequenceEqual(userLoginConfirmation.PasswordHash))
				return StatusCode(401, "Incorrect email or password");

			int userId = dapper.LoadDataSingle<int>($"EXEC TutorialAppSchema.spUsers_Get @Email = '{user.Email}'");
			return Ok(new Dictionary<string, string>() { { "token", authHelper.CreateToken(userId) } });
		}

		[HttpPut("/ResetPassword")]
		public IActionResult ResetPassword(UserLoginDto user)
		{
			if (authHelper.SetPassword(user)) return Ok();
			return StatusCode(500, "Failed to update password");
		}

		[HttpGet("/RefreshToken")]
		public IActionResult RefreshToken()
		{
			string userId = this.User.FindFirst("userId")?.Value ?? "";

			string userIdSql = $"SELECT UserId FROM TutorialAppSchema.Users WHERE UserId = {userId}";
			int userIdDb = dapper.LoadDataSingle<int>(userIdSql);
			return Ok(new Dictionary<string, string>() {
				{"token", authHelper.CreateToken(userIdDb)}
			});
		}
	}
}