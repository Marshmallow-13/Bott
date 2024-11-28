using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Bott.Core.Contracts;

[JsonObject(NamingStrategyType = typeof(SnakeCaseNamingStrategy))]
public record Chat(long Id, string FirstName, string Username, string Type);