namespace Horus.Shared.Models.Terminal;

public class TerminalSessionResult
{
    public bool Successful { get; }
    public List<string> ErrorLines { get; } = new();
    public List<string> OutputLines { get; }= new();

    public TerminalSessionResult()
    {
    }
    
    public TerminalSessionResult(bool successful, IEnumerable<string> errorLines, IEnumerable<string> outputLines)
    {
        Successful = successful;
        ErrorLines = errorLines.ToList();
        OutputLines = outputLines.ToList();
    }
}