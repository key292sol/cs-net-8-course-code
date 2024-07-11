using System;
using System.Data;
using System.Text;
using System.Globalization;
using Dapper;
using DbConn.Data;
using DbConn.Models;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Microsoft.Identity.Client;
using System.Text.Json;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using AutoMapper;

namespace DbConn
{
	internal class Program
	{

		static async Task Main(string[] args)
		{
			// IConfiguration configuration = new ConfigurationBuilder().AddJsonFile("appsettings.json").Build();
			// Dapper(configuration);
			// EntityFramework(configuration);
			// FileStuff();
			// JsonLecture();
			// MapModel();
			// await TasksLecture();
			await AsyncLecture();
		}

		static async Task AsyncLecture()
		{
			Task firstTask = new Task(() =>
			{
				Thread.Sleep(200);
				Console.WriteLine("Task 1");
			});
			firstTask.Start();
			Task task2 = ConsoleAfterAsync("Task 2", 300);

			ConsoleAfterDelay("Delay", 75);

			Task task3 = ConsoleAfterAsync("Task 3", 100);

			await task2;
			await firstTask;
			Console.WriteLine("Main thread");
			await task3;
		}

		static async Task ConsoleAfterAsync(string op, int delay)
		{
			await Task.Delay(delay);
			Console.WriteLine(op);
		}

		static void ConsoleAfterDelay(string op, int delay)
		{
			Thread.Sleep(delay);
			Console.WriteLine(op);
		}

		static async Task TasksLecture()
		{
			Task firstTask = new Task(() =>
			{
				Thread.Sleep(200);
				Console.WriteLine("Task 1");
			});
			firstTask.Start();

			await firstTask;
			Console.WriteLine("Main thread");
		}

		static void MapModel()
		{
			string computersJson = File.ReadAllText("ComputersSnake.json");

			// By using JsonPropertyName in the class
			IEnumerable<Computer>? computersSystem = System.Text.Json.JsonSerializer.Deserialize<IEnumerable<Computer>>(computersJson);

			if (computersSystem != null)
			{
				foreach (Computer computer in computersSystem)
				{
					Console.Write(computer.Motherboard + "  ||  ");
				}
			}

			// by using AutoMapper
			Mapper mapper = new Mapper(new MapperConfiguration(config =>
			{
				config.CreateMap<ComputerSnake, Computer>()
						.ForMember(dest => dest.ComputerId, options => options.MapFrom(src => src.computer_id))
						.ForMember(dest => dest.Motherboard, options => options.MapFrom(src => src.motherboard))
						.ForMember(dest => dest.CPUCores, options => options.MapFrom(src => src.cpu_cores))
						.ForMember(dest => dest.HasWifi, options => options.MapFrom(src => src.has_wifi))
						.ForMember(dest => dest.HasLTE, options => options.MapFrom(src => src.has_lte))
						.ForMember(dest => dest.ReleaseDate, options => options.MapFrom(src => src.release_date))
						.ForMember(dest => dest.Price, options => options.MapFrom(src => src.price))
						.ForMember(dest => dest.VideoCard, options => options.MapFrom(src => src.video_card));
			}));

			IEnumerable<ComputerSnake>? computersSnakes = System.Text.Json.JsonSerializer.Deserialize<IEnumerable<ComputerSnake>>(computersJson);

			if (computersSnakes != null)
			{
				IEnumerable<Computer> computers = mapper.Map<IEnumerable<Computer>>(computersSnakes);
				foreach (Computer computer in computers)
				{
					Console.Write(computer.Motherboard + "  ||  ");
				}
			}
		}

		static void JsonLecture()
		{
			IConfiguration config = new ConfigurationBuilder().AddJsonFile("appsettings.json").Build();
			DataContextDapper db = new DataContextDapper(config);

			string computersJson = File.ReadAllText("Computers.json");

			JsonSerializerOptions jsonOptions = new JsonSerializerOptions()
			{
				PropertyNamingPolicy = JsonNamingPolicy.CamelCase
			};

			IEnumerable<Computer>? computersSystem = System.Text.Json.JsonSerializer.Deserialize<IEnumerable<Computer>>(computersJson, jsonOptions);
			IEnumerable<Computer>? computersNewton = JsonConvert.DeserializeObject<IEnumerable<Computer>>(computersJson);

			if (computersNewton != null)
			{
				foreach (Computer computer in computersNewton)
				{
					string insSql = @"INSERT INTO TutorialAppSchema.Computer 
					(Motherboard, CPUCores, HasWifi, HasLTE, ReleaseDate, Price, VideoCard) 
					VALUES ('" + escapeSingleQuote(computer.Motherboard)
							+ "','" + computer.CPUCores
							+ "','" + computer.HasWifi
							+ "','" + computer.HasLTE
							+ "','" + computer.ReleaseDate?.ToString("yyyy-MM-dd")
							+ "','" + computer.Price.ToString("0.00", CultureInfo.InvariantCulture)
							+ "','" + escapeSingleQuote(computer.VideoCard)
					+ "')";

					db.ExecuteSql(insSql);
				}
			}

			JsonSerializerSettings jsonSerializerSettings = new JsonSerializerSettings()
			{
				ContractResolver = new CamelCasePropertyNamesContractResolver()
			};

			// string compsStr = JsonConvert.SerializeObject(computersSystem, jsonSerializerSettings);
			// File.WriteAllText("json_comp_newtonsoft.json", compsStr);

			// compsStr = System.Text.Json.JsonSerializer.Serialize(computersNewton, jsonOptions);
			// File.WriteAllText("json_comp_system.json", compsStr);
		}

		static string escapeSingleQuote(string str)
		{
			string op = str.Replace("'", "''");
			return op;
		}

		static void FileStuff()
		{
			string text = "alkfhliabvaovihru";
			File.WriteAllText("log.txt", text + "\n");

			StreamWriter writer = new("log.txt", append: true);
			writer.WriteLine(text);
			writer.Close();

			Console.WriteLine(File.ReadAllText("log.txt"));
		}

		static void EntityFramework(IConfiguration config)
		{
			DataContextEF ef = new DataContextEF(config);

			Computer myComputer = new Computer()
			{
				Motherboard = "CAS87",
				HasWifi = true,
				HasLTE = false,
				ReleaseDate = DateTime.Now,
				Price = 540m,
				VideoCard = "RTX 20"
			};

			// ef.Add(myComputer);
			// ef.SaveChanges();

			IEnumerable<Computer>? computers = ef.Computer?.ToList<Computer>();

			Console.WriteLine("ID | Motherboard | CPUCores | Wifi | LTE    | Release     | Price | Video card ");
			if (computers != null)
			{
				foreach (Computer comp in computers)
				{
					Console.WriteLine("'" + comp.ComputerId
						+ "','" + comp.Motherboard
						+ "'\t    ,'" + comp.CPUCores
						+ "'\t,'" + comp.HasWifi
						+ "'\t,'" + comp.HasLTE
						+ "','" + comp.ReleaseDate?.ToString("yyyy-MM-dd")
						+ "','" + comp.Price.ToString("0.00", CultureInfo.InvariantCulture)
						+ "'  ,'" + comp.VideoCard
				+ "'");
				}
			}
		}

		static void Dapper(IConfiguration config)
		{
			DataContextDapper db = new DataContextDapper(config);

			DateTime rightNow = db.LoadDataSingle<DateTime>("SELECT GETDATE()");

			Console.WriteLine(rightNow);

			Computer myComputer = new Computer()
			{
				ComputerId = 0,
				Motherboard = "Z690",
				HasWifi = true,
				HasLTE = false,
				ReleaseDate = DateTime.Now,
				Price = 943.87m,
				VideoCard = "RTX 2060"
			};

			string insSql = @"INSERT INTO TutorialAppSchema.Computer 
					(Motherboard, CPUCores, HasWifi, HasLTE, ReleaseDate, Price, VideoCard) 
					VALUES ('" + myComputer.Motherboard
					+ "','" + myComputer.CPUCores
					+ "','" + myComputer.HasWifi
					+ "','" + myComputer.HasLTE
					+ "','" + myComputer.ReleaseDate?.ToString("yyyy-MM-dd")
					+ "','" + myComputer.Price.ToString("0.00", CultureInfo.InvariantCulture)
					+ "','" + myComputer.VideoCard
			+ "')";

			// int result = db.ExecuteSql(insSql);
			// Console.WriteLine(result + " rows affected");
			// Console.WriteLine(insSql);

			string sqlSelect = "SELECT * FROM TutorialAppSchema.Computer";
			IEnumerable<Computer> computers = db.LoadData<Computer>(sqlSelect);

			Console.WriteLine("ID | Motherboard | CPUCores | Wifi | LTE    | Release     | Price | Video card ");
			foreach (Computer comp in computers)
			{
				Console.WriteLine("'" + comp.ComputerId
					+ "','" + comp.Motherboard
					+ "'\t    ,'" + comp.CPUCores
					+ "'\t,'" + comp.HasWifi
					+ "'\t,'" + comp.HasLTE
					+ "','" + comp.ReleaseDate?.ToString("yyyy-MM-dd")
					+ "','" + comp.Price.ToString("0.00", CultureInfo.InvariantCulture)
					+ "'  ,'" + comp.VideoCard
			+ "'");
			}
		}
	}
}