using WinPass.Shared.Models.Abstractions;

namespace WinPass.Shared.Models.Errors;

public class Error : IError
{
    public string Message { get; }

    public Error(string message)
    {
        Message = message;
    }
}