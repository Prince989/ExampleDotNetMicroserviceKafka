namespace SearchService.Application.Common;

public abstract class AppException : Exception
{
    public int StatusCode { get; }
    public string ErrorCode { get; }
    protected AppException(string message, int statusCode, string errorCode)
        : base(message) => (StatusCode, ErrorCode) = (statusCode, errorCode);
}

public sealed class NotFoundException    : AppException { public NotFoundException(string msg, string code="not_found")      : base(msg, 404, code) {} }
public sealed class ValidationException  : AppException { public ValidationException(string msg, string code="validation")    : base(msg, 400, code) {} }
public sealed class ConflictException    : AppException { public ConflictException(string msg, string code="conflict")        : base(msg, 409, code) {} }
public sealed class ForbiddenException   : AppException { public ForbiddenException(string msg, string code="forbidden")      : base(msg, 403, code) {} }
public sealed class UnauthorizedException: AppException { public UnauthorizedException(string msg, string code="unauthorized"): base(msg, 401, code) {} }