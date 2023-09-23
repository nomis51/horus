using WinPass.Shared.Models.Abstractions;

namespace WinPass.Shared.Models.Errors.Gpg;

public class GpgNotInstalledError : Error
{
    public GpgNotInstalledError() : base("GPG is not installed")
    {
    }
}