namespace Bott.Core.Contracts.Responses;

public record ErrorResponse(bool Ok, string? Description, long? ErrorCode, ResponseParameters Parameters) : Response(Ok);