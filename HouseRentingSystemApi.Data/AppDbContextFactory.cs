using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace HouseRentingSystemApi.Data
{
	public class AppDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
	{
		public AppDbContext CreateDbContext(string[] args)
		{
			var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();
			var connectionString = "Server=localhost;Database=houses;User=sa;Password=YourPassword123!;TrustServerCertificate=True;Encrypt=False;";
			optionsBuilder.UseSqlServer(connectionString);

			return new AppDbContext(optionsBuilder.Options);
		}
	}
}
