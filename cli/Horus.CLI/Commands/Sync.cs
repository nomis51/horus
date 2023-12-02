using Horus.Commands.Abstractions;

namespace Horus.Commands;

public class Sync : ICommand
{
    #region Public methods

    public void Run(List<string> args)
    {
        // TODO: command that will perform a git pull and git push to sync the password store with the remote repository.
        throw new NotImplementedException();
    }

    #endregion
}