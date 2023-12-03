using Horus.Shared.Enums;

namespace Horus.Shared.Models.Abstractions;

public abstract class Error : IError
{
    public ErrorSeverity Severity { get; }
    public string Message { get; } = string.Empty;

    protected Error(ErrorSeverity severity = ErrorSeverity.Error)
    {
        Severity = severity;
    }

    protected Error(string message, ErrorSeverity severity = ErrorSeverity.Error)
    {
        Message = message;
        Severity = severity;
    }
}