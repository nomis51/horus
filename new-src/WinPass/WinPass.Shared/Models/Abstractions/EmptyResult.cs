namespace WinPass.Shared.Models.Abstractions;

public class EmptyResult : Tuple<Error>
{
    #region Constructors

#pragma warning disable CS8625
    public EmptyResult() : base(default)
#pragma warning restore CS8625
    {
    }

#pragma warning disable CS8625
    public EmptyResult(Error error) : base(error)
#pragma warning restore CS8625
    {
    }

    #endregion
}