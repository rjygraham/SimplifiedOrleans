using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Orleans;
using SimplifiedOrleans.Abstractions;
using SimplifiedOrleans.Models;

namespace SimplifiedOrleans.Controllers
{
	[Route("[controller]")]
	[ApiController]
	public class CustomerController : ControllerBase
	{
		private readonly IGrainFactory grainFactory;
		private readonly ILogger logger;

		public CustomerController(IGrainFactory grainFactory, ILogger<CustomerController> logger)
		{
			this.grainFactory = grainFactory;
			this.logger = logger;
		}

		[HttpPost]
		public async Task<IActionResult> Post([FromBody] CustomerModel model)
		{
			var id = Guid.NewGuid();
			var customer = grainFactory.GetGrain<ICustomerGrain>(id);
			await customer.CreateAsync(model.Name);

			model.Id = id;

			return Created($"customer/{id}", model);
		}

		[HttpGet("{id}")]
		public async Task<IActionResult> Get(Guid id)
		{
			var customer = grainFactory.GetGrain<ICustomerGrain>(id);
			var name = await customer.GetDetailsAsync();

			if (string.IsNullOrEmpty(name))
			{
				return NotFound();
			}

			return Ok(new CustomerModel
			{
				Id = id,
				Name = name
			});
		}

		[HttpGet("{id}/sensors")]
		public async Task<IActionResult> GetSensors(Guid id)
		{
			var customer = grainFactory.GetGrain<ICustomerGrain>(id);
			var sensors = await customer.GetActiveSensors();
			return Ok(sensors);
		}

	}
}
