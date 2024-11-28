namespace Bott.Core.Contracts.Responses;

public record SuccessfulResponse<T>(bool Ok, T Result) : Response(Ok);