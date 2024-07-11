using AutoMapper;
using DotnetAPI.Data;
using DotnetAPI.DTO;
using DotnetAPI.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DotnetAPI.Controllers;

[ApiController]
[Route("[controller]")]
public class UserEFController : ControllerBase
{
	private IUserRepository userRepository;
	IMapper mapper = new Mapper(new MapperConfiguration(cfg =>
	{
		cfg.CreateMap<UserAddDTO, User>();
	}));

	public UserEFController(IConfiguration config, IUserRepository _userRepository)
	{
		// config.GetConnectionString("Default");
		userRepository = _userRepository;
	}

	[HttpGet("/UserEf/GetUsers")]
	public IEnumerable<User> GetUsers()
	{
		return userRepository.GetUsers();
	}

	[HttpGet("/UserEf/GetUser/{userId}")]
	public User? GetSingleUser(int userId)
	{
		return userRepository.GetSingleUser(userId);
	}

	[HttpPut("/UserEf/EditUser")]
	public IActionResult EditUser(User user)
	{
		User? userDb = userRepository.GetSingleUser(user.UserId);
		if (userDb != null)
		{
			userDb.FirstName = user.FirstName;
			userDb.LastName = user.LastName;
			userDb.Email = user.Email;
			userDb.Gender = user.Gender;
			userDb.Active = user.Active;

			if (userRepository.SaveChanges())
			{
				return Ok();
			}
			throw new Exception("Failed to Update User");
		}

		throw new Exception("Failed to Find User");
	}

	[HttpPost("/UserEf/AddUser")]
	public IActionResult AddUser(UserAddDTO user)
	{
		User userDb = mapper.Map<User>(user);
		userRepository.AddEntity<User>(userDb);

		if (userRepository.SaveChanges())
		{
			return Ok();
		}
		throw new Exception("Failed to Add User");
	}

	[HttpDelete("/UserEf/DeleteUser/{userId}")]
	public IActionResult DeleteUser(int userId)
	{
		User? userDb = userRepository.GetSingleUser(userId);

		if (userDb != null)
		{
			userRepository.RemoveEntity<User>(userDb);

			if (userRepository.SaveChanges()) return Ok();
			throw new Exception("Failed to Delete User");
		}

		throw new Exception("Failed to Find User");
	}
}