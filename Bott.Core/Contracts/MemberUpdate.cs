using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Bott.Core.Contracts;

[JsonObject(NamingStrategyType = typeof(SnakeCaseNamingStrategy))]
public record MemberUpdate(Chat Chat, User From, long Date, ChatMember OldChatMember, ChatMember NewChatMember);