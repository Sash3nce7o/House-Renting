using HouseRentingSystemApi.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HouseRentingSystemApi.Data.Configurations
{
	public class HouseConfiguration : IEntityTypeConfiguration<House>
	{
		public void Configure(EntityTypeBuilder<House> builder)
		{
			// No seeding - keep it empty
		}
	}
}
