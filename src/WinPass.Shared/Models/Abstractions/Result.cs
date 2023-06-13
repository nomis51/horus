namespace WinPass.Shared.Models.Abstractions;

public class Result<TResult, TError> : Tuple<TResult, TError>
    where TResult : class?
    where TError : class?, IError?
{
    #region Constructors

#pragma warning disable CS8625
    public Result(TResult result) : base(result, default)
#pragma warning restore CS8625
    {
    }

#pragma warning disable CS8625
    public Result(TError error) : base(default, error)
#pragma warning restore CS8625
    {
    }

    #endregion
}