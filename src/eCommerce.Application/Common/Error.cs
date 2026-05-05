namespace eCommerce.Application.Common;

public sealed record Error(
    string Code,
    string Message,
    ErrorType Type,
    IReadOnlyList<string>? Errors = null);
