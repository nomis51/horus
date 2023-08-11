namespace WinPass.Commands.Abstractions;

public interface ICommand
{
    void Run(List<string> args);
}