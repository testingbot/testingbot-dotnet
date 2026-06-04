using System.Collections.Generic;
using System.Net;
using System.Text.Json;

namespace TestingBot.Api.Serialization;

/// <summary>
/// Translates an unsuccessful HTTP response body into a typed <see cref="TestingBotException"/>.
/// Tolerates every error envelope the API is known to emit:
/// <c>{ "error": "..." }</c>, <c>{ "message": "..." }</c>,
/// <c>{ "success": false, "errors": [ ... ] }</c>, and <c>{ "errors": "&lt;json-string&gt;" }</c>.
/// </summary>
internal static class TestingBotErrorParser
{
    public static TestingBotException CreateException(
        HttpStatusCode statusCode,
        string? rawBody,
        TimeSpan? retryAfter,
        string requestMethod,
        Uri? requestUri)
    {
        var (apiMessage, errorCode, validationErrors) = Parse(rawBody);
        var message = BuildMessage(statusCode, apiMessage, requestMethod, requestUri);

        // The shared diagnostic context lives on init-only members of the base type. Construct each
        // exception with its full context up front so callers see a fully populated exception.
        return statusCode switch
        {
            HttpStatusCode.Unauthorized => new TestingBotAuthenticationException(message)
            {
                StatusCode = statusCode,
                ApiMessage = apiMessage,
                ErrorCode = errorCode,
                RawBody = rawBody,
                RequestMethod = requestMethod,
                RequestUri = requestUri,
                ValidationErrors = validationErrors,
            },
            HttpStatusCode.PaymentRequired => new TestingBotPaymentRequiredException(message)
            {
                StatusCode = statusCode,
                ApiMessage = apiMessage,
                ErrorCode = errorCode,
                RawBody = rawBody,
                RequestMethod = requestMethod,
                RequestUri = requestUri,
                ValidationErrors = validationErrors,
            },
            HttpStatusCode.Forbidden => new TestingBotForbiddenException(message)
            {
                StatusCode = statusCode,
                ApiMessage = apiMessage,
                ErrorCode = errorCode,
                RawBody = rawBody,
                RequestMethod = requestMethod,
                RequestUri = requestUri,
                ValidationErrors = validationErrors,
            },
            HttpStatusCode.NotFound => new TestingBotNotFoundException(message)
            {
                StatusCode = statusCode,
                ApiMessage = apiMessage,
                ErrorCode = errorCode,
                RawBody = rawBody,
                RequestMethod = requestMethod,
                RequestUri = requestUri,
                ValidationErrors = validationErrors,
            },
            HttpStatusCode.BadRequest => new TestingBotValidationException(message)
            {
                StatusCode = statusCode,
                ApiMessage = apiMessage,
                ErrorCode = errorCode,
                RawBody = rawBody,
                RequestMethod = requestMethod,
                RequestUri = requestUri,
                ValidationErrors = validationErrors,
            },
            HttpStatusCode.TooManyRequests => new TestingBotRateLimitException(message)
            {
                RetryAfter = retryAfter,
                StatusCode = statusCode,
                ApiMessage = apiMessage,
                ErrorCode = errorCode,
                RawBody = rawBody,
                RequestMethod = requestMethod,
                RequestUri = requestUri,
                ValidationErrors = validationErrors,
            },
            _ => new TestingBotApiException(message)
            {
                StatusCode = statusCode,
                ApiMessage = apiMessage,
                ErrorCode = errorCode,
                RawBody = rawBody,
                RequestMethod = requestMethod,
                RequestUri = requestUri,
                ValidationErrors = validationErrors,
            },
        };
    }

    private static (string? Message, string? ErrorCode, IReadOnlyList<string> ValidationErrors) Parse(string? rawBody)
    {
        if (string.IsNullOrWhiteSpace(rawBody))
        {
            return (null, null, []);
        }

        try
        {
            using var document = JsonDocument.Parse(rawBody);
            var root = document.RootElement;
            if (root.ValueKind != JsonValueKind.Object)
            {
                return (TrimToNull(rawBody), null, []);
            }

            string? message = null;
            if (root.TryGetProperty("error", out var error) && error.ValueKind == JsonValueKind.String)
            {
                message = error.GetString();
            }
            else if (root.TryGetProperty("message", out var msg) && msg.ValueKind == JsonValueKind.String)
            {
                message = msg.GetString();
            }

            string? errorCode = null;
            if (root.TryGetProperty("error_code", out var code) && code.ValueKind == JsonValueKind.String)
            {
                errorCode = code.GetString();
            }

            var validationErrors = ExtractErrors(root);
            if (message is null && validationErrors.Count > 0)
            {
                message = string.Join("; ", validationErrors);
            }

            return (TrimToNull(message), errorCode, validationErrors);
        }
        catch (JsonException)
        {
            return (TrimToNull(rawBody), null, []);
        }
    }

    private static List<string> ExtractErrors(JsonElement root)
    {
        if (!root.TryGetProperty("errors", out var errors))
        {
            return [];
        }

        switch (errors.ValueKind)
        {
            case JsonValueKind.Array:
                return ReadStringArray(errors);
            case JsonValueKind.String:
                var raw = errors.GetString();
                if (string.IsNullOrWhiteSpace(raw))
                {
                    return [];
                }

                // The API sometimes embeds a JSON-encoded array/object inside the string.
                try
                {
                    using var nested = JsonDocument.Parse(raw);
                    return nested.RootElement.ValueKind switch
                    {
                        JsonValueKind.Array => ReadStringArray(nested.RootElement),
                        JsonValueKind.Object => ReadObjectMessages(nested.RootElement),
                        _ => [raw],
                    };
                }
                catch (JsonException)
                {
                    return [raw];
                }

            case JsonValueKind.Object:
                return ReadObjectMessages(errors);
            default:
                return [];
        }
    }

    private static List<string> ReadStringArray(JsonElement array)
    {
        var list = new List<string>();
        foreach (var item in array.EnumerateArray())
        {
            var value = item.ValueKind == JsonValueKind.String ? item.GetString() : item.ToString();
            if (!string.IsNullOrWhiteSpace(value))
            {
                list.Add(value);
            }
        }

        return list;
    }

    private static List<string> ReadObjectMessages(JsonElement obj)
    {
        var list = new List<string>();
        foreach (var property in obj.EnumerateObject())
        {
            if (property.Value.ValueKind == JsonValueKind.Array)
            {
                foreach (var detail in ReadStringArray(property.Value))
                {
                    list.Add($"{property.Name}: {detail}");
                }
            }
            else
            {
                list.Add($"{property.Name}: {property.Value}");
            }
        }

        return list;
    }

    private static string BuildMessage(HttpStatusCode statusCode, string? apiMessage, string requestMethod, Uri? requestUri)
    {
        var detail = string.IsNullOrWhiteSpace(apiMessage)
            ? $"The TestingBot API returned status {(int)statusCode} ({statusCode})."
            : apiMessage!;
        return $"{requestMethod} {requestUri}: {detail}";
    }

    private static string? TrimToNull(string? value)
        => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}
