namespace DanmaobErp.Api.Errors;

public sealed record ApiError(int StatusCode, string Code, string MessageKey);
