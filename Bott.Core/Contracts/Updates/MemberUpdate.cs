namespace Bott.Core.Contracts.Updates;

public record MemberUpdate(long UpdateId, Contracts.MemberUpdate MyChatMember) : Update(UpdateId);