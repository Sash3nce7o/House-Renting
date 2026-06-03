
using HouseRentingSystemApi.Data;
using HouseRentingSystemApi.Data.Entities;
using HouseRentingSystemApi.Middleware;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace HouseRentingSystemApi
{
	public class Program
	{
		public static async Task Main(string[] args)
		{
			var builder = WebApplication.CreateBuilder(args);

			// Add services to the container.

			builder.Services.AddControllers();

			builder.Services.AddCors(options => options.AddPolicy("FrontendPolicy", policy =>
			{
				policy
					.WithOrigins("http://localhost:5173")
					.AllowAnyHeader()
					.AllowAnyMethod();
			}
			));
			// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
			builder.Services.AddEndpointsApiExplorer();
			builder.Services.AddSwaggerGen();

		var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

		
		builder.Services.AddDbContext<AppDbContext>(options => 
		{
			options.UseSqlServer(connectionString);
			options.ConfigureWarnings(w => w.Ignore(Microsoft.EntityFrameworkCore.Diagnostics.RelationalEventId.PendingModelChangesWarning));
		});
			
			builder.Services.AddIdentity<AppUser,IdentityRole>(opt =>
			{
				opt.SignIn.RequireConfirmedEmail = false;
				opt.Password.RequireNonAlphanumeric = false;
				opt.Password.RequiredLength = 6;
				opt.Password.RequireLowercase = false;
				opt.Password.RequireUppercase = false;

			})
				.AddEntityFrameworkStores<AppDbContext>()
				.AddDefaultTokenProviders();
			//---NEW SECTION---
			var jwtSection = builder.Configuration.GetSection("Jwt");
			var key = jwtSection["Key"];

			builder.Services
				.AddAuthentication(options =>
				{
					options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
					options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
				})
				.AddJwtBearer(options =>
				{
					options.TokenValidationParameters = new TokenValidationParameters
					{
						ValidateIssuer = true,
						ValidateAudience = true,
						ValidateLifetime = true,
						ValidateIssuerSigningKey = true,

						ValidIssuer = jwtSection["Issuer"],
						ValidAudience = jwtSection["Audience"],
						IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key!))
					};
				});
		builder.Services.AddAuthorization();
		//--END NEW SECTION--
		var app = builder.Build();

		// Configure the HTTP request pipeline.
		if (app.Environment.IsDevelopment())
		{
			app.UseSwagger();
			app.UseSwaggerUI();
		}
		app.UseCors("FrontendPolicy");
		app.UseHttpsRedirection();
		app.UseMiddleware<StopWatch>();

		app.UseAuthentication();
		app.UseAuthorization();

		// Auto-create database and seed roles
		using (var scope = app.Services.CreateScope())
		{
			var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
			db.Database.EnsureCreated();

			var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
			await SeedRoles(roleManager);
		}

		app.MapControllers();

		app.Run();
		}

		private static async Task SeedRoles(RoleManager<IdentityRole> roleManager)
		{
			string[] roleNames = ["Admin", "User"];

			foreach (var roleName in roleNames)
			{
				if (!await roleManager.RoleExistsAsync(roleName))
				{
					await roleManager.CreateAsync(new IdentityRole(roleName));
				}
			}
		}
	}
}
