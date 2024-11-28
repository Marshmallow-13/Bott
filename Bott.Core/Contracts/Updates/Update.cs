using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Bott.Core.Contracts.Updates;

[JsonObject(NamingStrategyType = typeof(SnakeCaseNamingStrategy))]
public record Update(long UpdateId);
