namespace Horus.Shared.Models.Abstractions;

public class ResultStruct<TResult, TError> : Tuple<TResult, TError>
    where TResult : struct
    where TError : class?, IError?
{
    #region Constructors

#pragma warning disable CS8625
    public ResultStruct(TResult result) : base(result, default)
#pragma warning restore CS8625
    {
    }

#pragma warning disable CS8625
    public ResultStruct(TError error) : base(default, error)
#pragma warning restore CS8625
    {
    }

    #endregion
}