namespace Bott.Core.Contracts.Updates;

public record CreateMessage(long UpdateId, Message Message) : Update(UpdateId);