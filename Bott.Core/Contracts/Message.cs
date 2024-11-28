using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Bott.Core.Contracts;

[JsonObject(NamingStrategyType = typeof(SnakeCaseNamingStrategy))]
public record Message([JsonProperty("message_id")] long Id, long? MessageThreadId, User? From, Chat Chat, string Text, long Date, MessageEntity[]? Entities);