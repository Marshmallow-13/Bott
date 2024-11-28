using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Bott.Core.Contracts;

[JsonObject(NamingStrategyType = typeof(SnakeCaseNamingStrategy))]
public record MessageEntity(int Offset, int Length, string Type, string? Url, string? Language, string? CustomEmojiId);