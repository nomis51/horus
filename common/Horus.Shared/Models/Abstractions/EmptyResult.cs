namespace Horus.Shared.Models.Abstractions;

public class EmptyResult
{
    public Error? Error { get; }
    public bool HasError => Error is not null;

    #region Constructors

    public EmptyResult()
    {
        Error = null;
    }

    public EmptyResult(Error error)
    {
        Error = error;
    }

    #endregion
}