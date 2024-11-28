using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Bott.Core.Contracts.Responses;

[JsonObject(NamingStrategyType = typeof(SnakeCaseNamingStrategy))]
public record Response(bool Ok);