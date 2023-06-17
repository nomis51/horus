using System.Reflection;
using System.Text;

var assembly = Assembly.GetEntryAssembly()!;
var tests = assembly.ExportedTypes.Where(t => t.Namespace == "WinPass.Tests.Tests" && t.Name.EndsWith("Tests"));
var output = new StringBuilder();
var consoleOut = Console.Out;
var stdout = new StringBuilder();
Console.SetOut(new StringWriter(stdout));

foreach (var test in tests)
{
    output.AppendLine($"Executing test {test.Name}");
    var testInstance = Activator.CreateInstance(test, stdout)!;
    var facts = test.GetMethods(BindingFlags.Public | BindingFlags.Instance).Where(m => m.Name.StartsWith("Should"));

    foreach (var fact in facts)
    {
        output.AppendLine($"Executing fact {fact.Name}");
        fact.Invoke(testInstance, null);
    }

    output.AppendLine();
}

Console.SetOut(consoleOut);
Console.Write(output.ToString());