using System;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Orleans;
using SimplifiedOrleans.Abstractions;
using SimplifiedOrleans.Entities;
using SimplifiedOrleans.Models;

namespace SimplifiedOrleans.Controllers
{
	[Route("[controller]")]
	[ApiController]
	public class SensorController : ControllerBase
	{
		private readonly IGrainFactory grainFactory;
		private readonly ILogger<SensorController> logger;

		public SensorController(IGrainFactory grainFactory, ILogger<SensorController> logger)
		{
			this.grainFactory = grainFactory;
			this.logger = logger;
		}

		[HttpPost("{id}")]
		public async Task<IActionResult> Post(Guid id, [FromBody] SensorConfigurationModel model)
		{
			try
			{
				var sensorConfiguration = new SensorConfiguration 
				{ 
					CustomerId = model.CustomerId, 
					ReadingWindow = model.ReadingWindow 
				};

				var sensor = grainFactory.GetGrain<ISensorGrain>(id);
				await sensor.ProvisionAsync(sensorConfiguration);
				model.Id = id;

				return Created($"sensor/{id}", model);
			}
			catch (Exception ex)
			{
				logger.LogError(ex.Message);
				return StatusCode((int)HttpStatusCode.InternalServerError);
			}
		}

		[HttpGet]
		public async Task<IActionResult> Get([FromQuery] ReadingModel model)
		{
			try
			{
				var entity = new SensorReading
				{
					Timestamp = DateTimeOffset.FromUnixTimeSeconds(model.Ts),
					Type = model.Type,
					ScanType = model.ScanType,
					Value = model.Value
				};

				var sensor = grainFactory.GetGrain<ISensorGrain>(model.Id);
				await sensor.RecordReadingAsync(entity);

				return Accepted();
			}
			catch (Exception ex)
			{
				logger.LogError(ex.Message);
				return StatusCode((int)HttpStatusCode.InternalServerError);
			}
		}

		[HttpGet("{id}")]
		public async Task<IActionResult> Get(Guid id)
		{
			try
			{
				var sensor = grainFactory.GetGrain<ISensorGrain>(id);
				var configuration = await sensor.GetConfiguration();

				var model = new SensorConfigurationModel 
				{
					Id = id,
					CustomerId = configuration.CustomerId,
					ReadingWindow = configuration.ReadingWindow,
					IsProvisioned = configuration.IsProvisioned
				};

				return Ok(model);
			}
			catch (Exception ex)
			{
				logger.LogError(ex.Message);
				return StatusCode((int)HttpStatusCode.InternalServerError);
			}
		}

		[HttpGet("{id}/readings/{timestamp}")]
		public async Task<IActionResult> GetReadings(Guid id, string timestamp)
		{
			try
			{
				var sensor = grainFactory.GetGrain<ISensorGrain>(id);

				if (long.TryParse(timestamp, out var ts))
				{
					var readings = await sensor.GetReadingsAfter(DateTimeOffset.FromUnixTimeSeconds(ts));

					return Ok(readings.Select(s => new ReadingModel
					{
						Id = id,
						Ts = s.Timestamp.ToUnixTimeSeconds(),
						ScanType = s.ScanType,
						Type = s.Type,
						Value = s.Value
					}));
				}
				else if (timestamp.Equals("current", StringComparison.OrdinalIgnoreCase))
				{
					var reading = await sensor.GetCurrentReadingAsync();

					return Ok(new ReadingModel
					{
						Id = id,
						ScanType = reading.ScanType,
						Type = reading.Type,
						Ts = reading.Timestamp.ToUnixTimeSeconds(),
						Value = reading.Value
					});
				}
				else
				{
					return BadRequest();
				}
			}
			catch (Exception ex)
			{
				logger.LogError(ex.Message);
				return StatusCode((int)HttpStatusCode.InternalServerError);
			}
		}

		[HttpDelete("{id}")]
		public async Task<IActionResult> Delete(Guid id)
		{
			try
			{
				var sensor = grainFactory.GetGrain<ISensorGrain>(id);
				await sensor.DeprovisionAsync();
				return NoContent();
			}
			catch (Exception ex)
			{
				logger.LogError(ex.Message);
				return StatusCode((int)HttpStatusCode.InternalServerError);
			}
		}
	}
}
