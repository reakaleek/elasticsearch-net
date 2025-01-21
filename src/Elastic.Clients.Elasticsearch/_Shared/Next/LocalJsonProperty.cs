// Licensed to Elasticsearch B.V under one or more agreements.
// Elasticsearch B.V licenses this file to you under the Apache 2.0 License.
// See the LICENSE file in the project root for more information.

using System;
using System.Runtime.CompilerServices;
using System.Text.Json;

namespace Elastic.Clients.Elasticsearch.Next;

/// <summary>
/// TODO: TBC
/// </summary>
/// <typeparam name="T"></typeparam>
internal ref struct LocalJsonProperty<T>
{
	public T? Value;
	public bool Initialized;

	/// <summary>
	/// TODO: TBC
	/// </summary>
	/// <param name="reader"></param>
	/// <param name="options"></param>
	/// <param name="name"></param>
	/// <param name="markerType"></param>
	/// <returns></returns>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public bool TryRead(ref Utf8JsonReader reader, JsonSerializerOptions options, JsonEncodedText name, Type? markerType = null)
	{
		var success = reader.TryReadProperty(options, name, ref Value, markerType);
		Initialized |= success;

		return success;
	}
}
