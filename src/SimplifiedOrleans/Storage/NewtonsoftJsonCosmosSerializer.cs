﻿using System.IO;
using System.Text;
using Microsoft.Azure.Cosmos;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace SimplifiedOrleans.Storage
{
	/// <summary>
	/// The default Cosmos JSON.NET serializer.
	/// </summary>
	internal sealed class NewtonsoftJsonCosmosSerializer : CosmosSerializer
	{
		private static readonly Encoding DefaultEncoding = new UTF8Encoding(false, true);
		private readonly JsonSerializer Serializer;

		/// <summary>
		/// Create a serializer that uses the JSON.net serializer
		/// </summary>
		/// <remarks>
		/// This is internal to reduce exposure of JSON.net types so
		/// it is easier to convert to System.Text.Json
		/// </remarks>
		internal NewtonsoftJsonCosmosSerializer()
		{
			this.Serializer = JsonSerializer.Create();
		}

		/// <summary>
		/// Create a serializer that uses the JSON.net serializer
		/// </summary>
		/// <remarks>
		/// This is internal to reduce exposure of JSON.net types so
		/// it is easier to convert to System.Text.Json
		/// </remarks>
		internal NewtonsoftJsonCosmosSerializer(CosmosSerializationOptions cosmosSerializerOptions)
		{
			JsonSerializerSettings jsonSerializerSettings = new JsonSerializerSettings()
			{
				NullValueHandling = cosmosSerializerOptions.IgnoreNullValues ? NullValueHandling.Ignore : NullValueHandling.Include,
				Formatting = cosmosSerializerOptions.Indented ? Formatting.Indented : Formatting.None,
				ContractResolver = cosmosSerializerOptions.PropertyNamingPolicy == CosmosPropertyNamingPolicy.CamelCase
					? new CamelCasePropertyNamesContractResolver()
					: null,
				TypeNameHandling = TypeNameHandling.Auto
			};

			this.Serializer = JsonSerializer.Create(jsonSerializerSettings);
		}

		/// <summary>
		/// Create a serializer that uses the JSON.net serializer
		/// </summary>
		/// <remarks>
		/// This is internal to reduce exposure of JSON.net types so
		/// it is easier to convert to System.Text.Json
		/// </remarks>
		internal NewtonsoftJsonCosmosSerializer(JsonSerializerSettings jsonSerializerSettings)
		{
			this.Serializer = JsonSerializer.Create(jsonSerializerSettings);
		}

		/// <summary>
		/// Convert a Stream to the passed in type.
		/// </summary>
		/// <typeparam name="T">The type of object that should be deserialized</typeparam>
		/// <param name="stream">An open stream that is readable that contains JSON</param>
		/// <returns>The object representing the deserialized stream</returns>
		public override T FromStream<T>(Stream stream)
		{
			using (stream)
			{
				if (typeof(Stream).IsAssignableFrom(typeof(T)))
				{
					return (T)(object)stream;
				}

				using (StreamReader sr = new StreamReader(stream))
				{
					using (JsonTextReader jsonTextReader = new JsonTextReader(sr))
					{
						return this.Serializer.Deserialize<T>(jsonTextReader);
					}
				}
			}
		}

		/// <summary>
		/// Converts an object to a open readable stream
		/// </summary>
		/// <typeparam name="T">The type of object being serialized</typeparam>
		/// <param name="input">The object to be serialized</param>
		/// <returns>An open readable stream containing the JSON of the serialized object</returns>
		public override Stream ToStream<T>(T input)
		{
			MemoryStream streamPayload = new MemoryStream();
			using (StreamWriter streamWriter = new StreamWriter(streamPayload, encoding: NewtonsoftJsonCosmosSerializer.DefaultEncoding, bufferSize: 1024, leaveOpen: true))
			{
				using (JsonWriter writer = new JsonTextWriter(streamWriter))
				{
					writer.Formatting = Newtonsoft.Json.Formatting.None;
					this.Serializer.Serialize(writer, input);
					writer.Flush();
					streamWriter.Flush();
				}
			}

			streamPayload.Position = 0;
			return streamPayload;
		}
	}
}
