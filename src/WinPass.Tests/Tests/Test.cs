using System.Text;

namespace WinPass.Tests.Tests;

public abstract class Test
{
    #region Members

    protected readonly StringBuilder Stdout;

    #endregion

    #region Constructors

    protected Test(StringBuilder stdout)
    {
        Stdout = stdout;
    }

    #endregion
}