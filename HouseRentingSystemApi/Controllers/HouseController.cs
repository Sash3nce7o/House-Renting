using HouseRentingSystemApi.Data;
using HouseRentingSystemApi.Data.Entities;
using HouseRentingSystemApi.Models;
using HouseRentingSystemApi.Models.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Identity.Client;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace HouseRentingSystemApi.Controllers
{
	[Route("api/[controller]")]
	public class HouseController : ControllerBase
	{
		private AppDbContext context;

		public HouseController(AppDbContext context)
		{
			this.context = context;
		}

		[HttpGet("All")]
		[Produces(typeof(IEnumerable<HouseDetailModel>))]
		public async Task<IActionResult> GetAll()
		{
			var model = await context.Houses
				.AsNoTracking()
				.Select(h => new HouseDetailModel()
				{
					
					Title = h.Title,
					Address = h.Address,
					ImageUrl = h.ImageUrl
				})
				.ToListAsync();

			return Ok(model);
		}

		[HttpGet("search-by-category")]
		[Produces(typeof(IEnumerable<HouseDetailModel>))]
		public async Task<IActionResult> SearchByCategory(int categoryId, string sortOrder = "asc")
		{
			if (sortOrder != "asc" && sortOrder != "desc")
			{
				return BadRequest(new { message = "sortOrder must be 'asc' or 'desc'" });
			}

			var query = context.Houses
				.AsNoTracking()
				.Where(h => h.CategoryId == categoryId && !h.isDeleted);

			if (sortOrder == "asc")
			{
				query = query.OrderBy(h => h.DeletedOnUtc);
			}
			else
			{
				query = query.OrderByDescending(h => h.DeletedOnUtc);
			}

			var model = await query
				.Select(h => new HouseDetailModel()
				{
					Title = h.Title,
					Address = h.Address,
					ImageUrl = h.ImageUrl,
					Description = h.Description,
					PricePerMonth = h.PricePerMonth,
					Category = (CategoryViewEnum)h.CategoryId
				})
				.ToListAsync();

			return Ok(model);
		}

		[HttpGet("me")]
		public IActionResult Me()
		{
			var isAuthenticated = User.Identity?.IsAuthenticated;
			var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

			return Ok(new { isAuthenticated, userId });
		}

		[HttpGet("{id}")]
		[Produces(typeof(HouseDetailModel))]
		public async Task<IActionResult> GetById(int id)
		{
			var house = await context.Houses.FirstOrDefaultAsync(h => h.Id == id);
			if (house == null)
			{
				return NotFound();
			}

			return Ok(new HouseDetailModel()
			{
				Title = house.Title,
				Address = house.Address,
				ImageUrl = house.ImageUrl
			});
		}

		[HttpPost("All")]
		[Authorize]
		[Produces(typeof(HouseDetailModel))]
		public async Task<IActionResult> Create([FromBody]HouseDetailModel model)
		{
			if (ModelState.IsValid == false)
			{
				return BadRequest();
			}
			var isAuthenticated = User.Identity?.IsAuthenticated;
			var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

			var newHouse = new House()
			{
				Description = model.Description,
				PricePerMonth = model.PricePerMonth,
				Address = model.Address,
				Title=model.Title,

				ImageUrl =model.ImageUrl
			};

			//var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
			var userEmail = User.FindFirstValue(ClaimTypes.Email);

			var category = await context.Categories
				.FirstOrDefaultAsync(c => c.Name == model.Category
				.ToString());
			if (category == null) 
			{
				var newCategory = new Category()
				{
					Name = model.Category.ToString(),
				};
				context.Categories.Add(newCategory);
				await context.SaveChangesAsync();
				newHouse.CategoryId = newCategory.Id;
			}
			else
			{
				newHouse.CategoryId = category.Id;
			}
			newHouse.UserId = userId;
			context.Houses.Add(newHouse);
			await context.SaveChangesAsync();

			return Created($"api/All/{newHouse.Id}",new HouseDetailModel() 
			{ 
				Address = newHouse.Address,
				ImageUrl = newHouse.ImageUrl,
				Title = newHouse.Title,
				Description = newHouse.Description,
				PricePerMonth = newHouse.PricePerMonth,
				Category = model.Category
			});
		}

		[HttpPut("{id}")]
		[Authorize]
		public async Task<IActionResult> UpdateHouse(int id,HouseDetailModel model)
		{
			if (ModelState.IsValid == false)
			{
				return BadRequest();
			}

			var house = await context.Houses.FirstOrDefaultAsync(h => h.Id == id);
			if (house == null)
			{
				return NotFound();
			}

			house.Title = model.Title;
			house.Address = model.Address;
			house.ImageUrl = model.ImageUrl;
			house.Description = model.Description;
			house.PricePerMonth = model.PricePerMonth;

			var category = await context.Categories
				.FirstOrDefaultAsync(c => c.Name == model.Category.ToString());
			if (category == null)
			{
				var newCategory = new Category
				{
					Name = model.Category.ToString(),
				};

				context.Categories.Add(newCategory);
				await context.SaveChangesAsync();
				house.CategoryId = newCategory.Id;
			}
			else
			{
				house.CategoryId = category.Id;
			}

			await context.SaveChangesAsync();

			return Ok(new HouseDetailModel
			{
				Title = house.Title,
				Address = house.Address,
				ImageUrl = house.ImageUrl,
				Description = house.Description,
				PricePerMonth = house.PricePerMonth,
				Category = model.Category
			});
		}
		[HttpDelete("{id}")]
		[Authorize]
		public async Task<IActionResult> DeleteHouse(int id)
		{
			var house = await context.Houses.FindAsync(id);

			house!.isDeleted = true;
			house.DeletedOnUtc = DateTime.UtcNow;
			await context.SaveChangesAsync();
			return NoContent();
		}
	}
}
