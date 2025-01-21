// Licensed to Elasticsearch B.V under one or more agreements.
// Elasticsearch B.V licenses this file to you under the Apache 2.0 License.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Elastic.Clients.Elasticsearch.Next;

internal static class JsonWriterExtensions
{
    /// <summary>
    ///
    /// </summary>
    /// <param name="writer"></param>
    /// <param name="options"></param>
    /// <param name="name"></param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void WritePropertyName(this Utf8JsonWriter writer, JsonSerializerOptions options, JsonEncodedText name)
    {
        // This parameter is kept to allow implicit specialization of the `WritePropertyName<T>` method.
        _ = options;

        writer.WritePropertyName(name);
    }

    /// <summary>
    ///
    /// </summary>
    /// <param name="writer"></param>
    /// <param name="options"></param>
    /// <param name="name"></param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void WritePropertyName(this Utf8JsonWriter writer, JsonSerializerOptions options, string name)
    {
        // This parameter is kept to allow implicit specialization of the `WritePropertyName<T>` method.
        _ = options;

        writer.WritePropertyName(name);
    }

    /// <summary>
    ///
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="writer"></param>
    /// <param name="name"></param>
    /// <param name="options"></param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void WritePropertyName<T>(this Utf8JsonWriter writer, JsonSerializerOptions options, [DisallowNull] T name, Type? markerType = null)
    {
        options.GetConverter<T>(markerType).WriteAsPropertyName(writer, name, options);
    }

    /// <summary>
    /// Serializes the given <paramref name="value"/> to JSON using the appropriate <see cref="JsonConverter"/>.
    /// </summary>
    /// <typeparam name="T">The type of the value.</typeparam>
    /// <param name="writer">The <see cref="Utf8JsonWriter"/> to use for writing the serialized value.</param>
    /// <param name="value">The value to serialize.</param>
    /// <param name="options">The <see cref="JsonSerializerOptions"/> to use.</param>
    /// <param name="markerType">An optional type hint, used to retrieve a matching converter from the given <paramref name="options"/>.</param>
    /// <remarks>
    /// The matching converter for <paramref name="markerType"/> must implement <see cref="IMarkerTypeConverter"/>.
    /// </remarks>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void WriteValue<T>(this Utf8JsonWriter writer, JsonSerializerOptions options, T value, Type? markerType = null)
    {
        var converter = options.GetConverter<T>(markerType);

        if ((value is null) && !converter.HandleNull)
        {
            writer.WriteNullValue();
            return;
        }

        converter.Write(writer, value, options);
    }

    /// <summary>
    /// TODO: TBC
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="writer"></param>
    /// <param name="name"></param>
    /// <param name="value"></param>
    /// <param name="options"></param>
    /// <param name="nameMarkerType"></param>
    /// <param name="valueMarkerType"></param>
    /// <param name="ignoreCondition"></param>
    /// <exception cref="NotSupportedException"></exception>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void WriteProperty<T>(this Utf8JsonWriter writer, JsonSerializerOptions options, string name, T value,
        Type? nameMarkerType = null, Type? valueMarkerType = null, JsonIgnoreCondition? ignoreCondition = null)
    {
        // This parameter is kept to allow implicit specialization of the `WriteProperty<TName, TValue>` method.
        _ = nameMarkerType;

        var shouldIgnore = (ignoreCondition ?? options.DefaultIgnoreCondition) switch
        {
            JsonIgnoreCondition.Never => false,
            JsonIgnoreCondition.Always => true,
            JsonIgnoreCondition.WhenWritingDefault => EqualityComparer<T>.Default.Equals(value, default!),
            JsonIgnoreCondition.WhenWritingNull => (value is null),
            _ => throw new NotSupportedException("Unsupported JSON ignore condition.")
        };

        if (shouldIgnore)
        {
            return;
        }

        writer.WritePropertyName(options, name);
        writer.WriteValue(options, value, valueMarkerType);
    }

    /// <summary>
    /// TODO: TBC
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="writer"></param>
    /// <param name="name"></param>
    /// <param name="value"></param>
    /// <param name="options"></param>
    /// <param name="nameMarkerType"></param>
    /// <param name="valueMarkerType"></param>
    /// <param name="ignoreCondition"></param>
    /// <exception cref="NotSupportedException"></exception>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void WriteProperty<T>(this Utf8JsonWriter writer, JsonSerializerOptions options, JsonEncodedText name, T value,
        Type? nameMarkerType = null, Type? valueMarkerType = null, JsonIgnoreCondition? ignoreCondition = null)
    {
        // This parameter is kept to allow implicit specialization of the `WriteProperty<TName, TValue>` method.
        _ = nameMarkerType;

        var shouldIgnore = (ignoreCondition ?? options.DefaultIgnoreCondition) switch
        {
            JsonIgnoreCondition.Never => false,
            JsonIgnoreCondition.Always => true,
            JsonIgnoreCondition.WhenWritingDefault => EqualityComparer<T>.Default.Equals(value, default!),
            JsonIgnoreCondition.WhenWritingNull => (value is null),
            _ => throw new NotSupportedException("Unsupported JSON ignore condition.")
        };

        if (shouldIgnore)
        {
            return;
        }

        writer.WritePropertyName(options, name);
        writer.WriteValue(options, value, valueMarkerType);
    }

    /// <summary>
    ///
    /// </summary>
    /// <typeparam name="TName"></typeparam>
    /// <typeparam name="TValue"></typeparam>
    /// <param name="writer"></param>
    /// <param name="name"></param>
    /// <param name="value"></param>
    /// <param name="options"></param>
    /// <param name="nameMarkerType"></param>
    /// <param name="valueMarkerType"></param>
    /// <param name="ignoreCondition"></param>
    /// <exception cref="NotSupportedException"></exception>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void WriteProperty<TName, TValue>(this Utf8JsonWriter writer, JsonSerializerOptions options, [DisallowNull] TName name, TValue value,
        Type? nameMarkerType = null, Type? valueMarkerType = null, JsonIgnoreCondition? ignoreCondition = null)
    {
        var shouldIgnore = (ignoreCondition ?? options.DefaultIgnoreCondition) switch
        {
            JsonIgnoreCondition.Never => false,
            JsonIgnoreCondition.Always => true,
            JsonIgnoreCondition.WhenWritingDefault => EqualityComparer<TValue>.Default.Equals(value, default!),
            JsonIgnoreCondition.WhenWritingNull => (value is null),
            _ => throw new NotSupportedException("Unsupported JSON ignore condition.")
        };

        if (shouldIgnore)
        {
            return;
        }

        writer.WritePropertyName(options, name, nameMarkerType);
        writer.WriteValue(options, value, valueMarkerType);
    }
}
