using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Newtonsoft.Json;
using Orleans.Hosting;
using Orleans.Runtime;
using SimplifiedOrleans.Configuration;
using SimplifiedOrleans.Grains;
using SimplifiedOrleans.Models;
using SimplifiedOrleans.Storage;
using SimplifiedOrleans.Validators;

await Host.CreateDefaultBuilder(args)
	.UseOrleans((ctx, siloBuilder) =>
	{
		if (ctx.HostingEnvironment.IsDevelopment())
		{
			siloBuilder
				.UseLocalhostClustering();
		}
		else
		{
			// For hosting Azure we'll use AKS to orchestrate and Cosmos DB for Orleans clustering.
			siloBuilder
				.UseKubernetesHosting()
				.UseCosmosDBMembership(options =>
				{
					options.Bind(ctx.Configuration.GetSection("CosmosDb"));
				});
		}

		siloBuilder
			.AddApplicationInsightsTelemetryConsumer(ctx.Configuration.GetSection("ApplicationInsights").GetValue<string>("InstrumentationKey"))
			.AddCustomStorageBasedLogConsistencyProvider("CustomStorage")
			.ConfigureServices(s =>
			{
				var cosmosDbEventStoreOptions = new CosmosDbEventStoreOptions();
				ctx.Configuration.GetSection("CosmosDb").Bind(cosmosDbEventStoreOptions);

				var jsonSerializerSettings = new JsonSerializerSettings
				{
					NullValueHandling = NullValueHandling.Ignore,
					TypeNameHandling = TypeNameHandling.Auto
				};

				var cosmosOptions = new CosmosClientOptions
				{
					Serializer = new NewtonsoftJsonCosmosSerializer(jsonSerializerSettings),
					EnableContentResponseOnWrite = false
				};

				var client = new CosmosClient(cosmosDbEventStoreOptions.AccountEndpoint, cosmosDbEventStoreOptions.AccountKey, cosmosOptions);
				var eventStore = new CosmosDbEventStore(client, cosmosDbEventStoreOptions);

				s.AddSingletonNamedService<IEventStore>(nameof(CustomerGrain), (sp, n) => eventStore);
				s.AddSingletonNamedService<IEventStore>(nameof(SensorGrain), (sp, n) => eventStore);
			});
	})
	.ConfigureWebHostDefaults(webBuilder =>
	{
		webBuilder
			.ConfigureServices(services =>
			{
				services
					.AddControllers()
					.AddNewtonsoftJson()
					.AddFluentValidation();

				services.AddTransient<IValidator<CustomerModel>, CustomerModelValidator>();
				services.AddTransient<IValidator<SensorConfigurationModel>, SensorConfigurationModelValidator>();
			})
			.Configure((ctx, app) =>
			{
				if (ctx.HostingEnvironment.IsDevelopment())
				{
					app.UseDeveloperExceptionPage();
				}

				app.UseRouting();
				app.UseAuthorization();
				app.UseEndpoints(endpoints =>
				{
					endpoints.MapControllers();
				});
			});
	})
	.ConfigureServices(services => { })
	.RunConsoleAsync();
