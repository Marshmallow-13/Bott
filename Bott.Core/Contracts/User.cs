using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Bott.Core.Contracts;

[JsonObject(NamingStrategyType = typeof(SnakeCaseNamingStrategy))]
public record User(long Id, bool IsBot, string FirstName, string Username, string LanguageCode);