// Licensed to Elasticsearch B.V under one or more agreements.
// Elasticsearch B.V licenses this file to you under the Apache 2.0 License.
// See the LICENSE file in the project root for more information.

using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Text.Json;

namespace Elastic.Clients.Elasticsearch.Next;

internal static class JsonReaderExtensions
{
    //// If the exception source is this value, the serializer will re-throw as JsonException.
    //private const string ExceptionSourceValueToRethrowAsJsonException = "System.Text.Json.Rethrowable";

    /// <summary>
    /// TODO: TBC
    /// </summary>
    /// <param name="reader"></param>
    /// <param name="expected"></param>
    /// <exception cref="JsonException"></exception>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void ValidateToken(this ref Utf8JsonReader reader, JsonTokenType expected)
    {
        if (reader.TokenType != expected)
        {
            throw new JsonException($"Expected JSON '{expected}' token, but got '{reader.TokenType}'.");
        }
    }

    /// <summary>
    ///
    /// </summary>
    /// <param name="reader"></param>
    /// <param name="options"></param>
    /// <param name="name"></param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void ReadPropertyName(this ref Utf8JsonReader reader, JsonSerializerOptions options, out string name)
    {
        // This parameter is kept to allow implicit specialization of the `ReadPropertyName<T>` method.
        _ = options;

        Debug.Assert(reader.TokenType is JsonTokenType.PropertyName);

        name = reader.GetString()!;
    }

    /// <summary>
    ///
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="reader"></param>
    /// <param name="options"></param>
    /// <param name="markerType"></param>
    /// <returns></returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void ReadPropertyName<T>(this ref Utf8JsonReader reader, JsonSerializerOptions options, out T name, Type? markerType = null)
    {
        Debug.Assert(reader.TokenType is JsonTokenType.PropertyName);

        name = options.GetConverter<T>(markerType).ReadAsPropertyName(ref reader, typeof(T), options);
    }

    /// <summary>
    /// TODO: TBC
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="reader"></param>
    /// <param name="options"></param>
    /// <param name="markerType"></param>
    /// <returns></returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T? ReadValue<T>(this ref Utf8JsonReader reader, JsonSerializerOptions options, Type? markerType = null)
    {
        var converter = options.GetConverter<T>(markerType);

        if ((reader.TokenType is JsonTokenType.Null) && !converter.HandleNull)
        {
            return default;
        }

        return converter.Read(ref reader, typeof(T), options);
    }

    /// <summary>
    ///
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="reader"></param>
    /// <param name="options"></param>
    /// <param name="name"></param>
    /// <param name="value"></param>
    /// <param name="nameMarkerType"></param>
    /// <param name="valueMarkerType"></param>
    /// <returns></returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void ReadProperty<T>(this ref Utf8JsonReader reader, JsonSerializerOptions options, out string name, out T? value,
        Type? nameMarkerType = null, Type? valueMarkerType = null)
    {
        // This parameter is kept to allow implicit specialization of the `ReadProperty<TName, TValue>` method.
        _ = nameMarkerType;

        Debug.Assert(reader.TokenType is JsonTokenType.PropertyName);

        reader.ReadPropertyName(options, out name);
        reader.Read();
        value = reader.ReadValue<T>(options, valueMarkerType);
    }

    /// <summary>
    ///
    /// </summary>
    /// <typeparam name="TName"></typeparam>
    /// <typeparam name="TValue"></typeparam>
    /// <param name="reader"></param>
    /// <param name="options"></param>
    /// <param name="name"></param>
    /// <param name="value"></param>
    /// <param name="valueMarkerType"></param>
    /// <param name="nameMarkerType"></param>
    /// <returns></returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void ReadProperty<TName, TValue>(this ref Utf8JsonReader reader, JsonSerializerOptions options, [DisallowNull] out TName name, out TValue? value,
        Type? nameMarkerType = null, Type? valueMarkerType = null)
    {
        Debug.Assert(reader.TokenType is JsonTokenType.PropertyName);

        reader.ReadPropertyName(options, out name, nameMarkerType);
        reader.Read();
        value = reader.ReadValue<TValue>(options, valueMarkerType);
    }

    /// <summary>
    /// TODO: TBC
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="reader"></param>
    /// <param name="name"></param>
    /// <param name="options"></param>
    /// <param name="value"></param>
    /// <param name="markerType"></param>
    /// <returns></returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool TryReadProperty<T>(this ref Utf8JsonReader reader, JsonSerializerOptions options, JsonEncodedText name, ref T? value, Type? markerType = null)
    {
        Debug.Assert(reader.TokenType is JsonTokenType.PropertyName);

        if (!reader.ValueTextEquals(name))
        {
            return false;
        }

        reader.Read();
        value = reader.ReadValue<T>(options, markerType);

        return true;
    }

    /// <summary>
    /// Compares the JSON encoded text to the JSON token value in the source and returns true if they match.
    /// </summary>
    /// <param name="reader">A reference to the <see cref="Utf8JsonReader"/>.</param>
    /// <param name="text">The JSON encoded text to compare against.</param>
    /// <returns><see langword="true"/> if the JSON token value in the source matches the JSON encoded look up text.</returns>
    /// <remarks>
    ///     This is an alternative version of the built-in <see cref="Utf8JsonReader.ValueTextEquals(ReadOnlySpan{byte})"/> method
    ///     that operates on pre-encoded JSON text.
    /// </remarks>
    public static bool ValueTextEquals(this ref Utf8JsonReader reader, JsonEncodedText text)
    {
        Debug.Assert(reader.TokenType is JsonTokenType.PropertyName or JsonTokenType.String);

        return reader.HasValueSequence
            ? CompareToSequence(ref reader, text.EncodedUtf8Bytes)
            : reader.ValueSpan.SequenceEqual(text.EncodedUtf8Bytes);

        static bool CompareToSequence(ref Utf8JsonReader reader, ReadOnlySpan<byte> other)
        {
            var localSequence = reader.ValueSequence;
            if (localSequence.Length != other.Length)
            {
                return false;
            }

            var matchedSoFar = 0;

            foreach (var memory in localSequence)
            {
                var span = memory.Span;

                if (other[matchedSoFar..].StartsWith(span))
                {
                    matchedSoFar += span.Length;
                }
                else
                {
                    return false;
                }
            }

            return true;
        }
    }
}
