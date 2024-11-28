namespace Bott.Core.Contracts.Updates;

public record MessageCreate(long UpdateId, Message Message) : Update(UpdateId);