// Licensed to Elasticsearch B.V under one or more agreements.
// Elasticsearch B.V licenses this file to you under the Apache 2.0 License.
// See the LICENSE file in the project root for more information.

using System;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;

using Elastic.Transport.Extensions;

namespace Elastic.Clients.Elasticsearch.Next;

public sealed class RequestResponseMarker<T>
{
	static RequestResponseMarker()
	{
		DynamicallyAccessed.PublicConstructors(typeof(RequestResponseMarkerConverter<T>));
	}
}

internal sealed class RequestResponseMarkerConverter<T> :
	JsonConverter<RequestResponseMarker<T>>,
	IMarkerTypeConverter
{
	public JsonConverter WrappedConverter { get; }

	public RequestResponseMarkerConverter(IElasticsearchClientSettings settings)
	{
		WrappedConverter = new RequestResponseConverter<T>(settings);
	}

	public override RequestResponseMarker<T>? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
	{
		throw new NotImplementedException();
	}

	public override void Write(Utf8JsonWriter writer, RequestResponseMarker<T> value, JsonSerializerOptions options)
	{
		throw new NotImplementedException();
	}
}

internal sealed class RequestResponseConverterFactory :
	ContextAwareJsonConverterFactory<IElasticsearchClientSettings>
{
	private readonly IElasticsearchClientSettings _settings;

	public RequestResponseConverterFactory(IElasticsearchClientSettings settings) :
		base(settings)
	{
		_settings = settings;
	}

	public override bool CanConvert(Type typeToConvert)
	{
		return typeToConvert.IsGenericType &&
			   typeToConvert.GetGenericTypeDefinition() == typeof(RequestResponseMarker<>);
	}

	public override JsonConverter? CreateConverter(Type typeToConvert, JsonSerializerOptions options)
	{
		var args = typeToConvert.GetGenericArguments();

#pragma warning disable IL3050 // RequestResponseMarker<T> static constructor roots RequestResponseMarkerConverter<T>.

		var converter = (JsonConverter)Activator.CreateInstance(
			typeof(RequestResponseMarkerConverter<>).MakeGenericType(args[0]),
			BindingFlags.Instance | BindingFlags.Public,
			binder: null,
			args: [_settings],
			culture: null)!;

#pragma warning restore IL3050

		return converter;
	}
}

internal sealed class RequestResponseConverter<T> :
	JsonConverter<T>
{
	private readonly IElasticsearchClientSettings _settings;

	public RequestResponseConverter(IElasticsearchClientSettings settings)
	{
		_settings = settings;
	}

	public override bool CanConvert(Type typeToConvert)
	{
		return typeToConvert == typeof(RequestResponseMarker<T>);
	}

	public override T Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
	{
#pragma warning disable IL2026, IL3050
		return _settings.RequestResponseSerializer.Deserialize<T>(ref reader)!;
#pragma warning restore IL2026, IL3050
	}

	public override void Write(Utf8JsonWriter writer, T value, JsonSerializerOptions options)
	{
#pragma warning disable IL2026, IL3050
		_settings.RequestResponseSerializer.Serialize(value, writer);
#pragma warning restore IL2026, IL3050
	}
}
