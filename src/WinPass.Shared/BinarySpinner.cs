using System.Text;
using Spectre.Console;

namespace WinPass.Shared;

public sealed class BinarySpinner : Spinner
{
    #region Constants

    private const int NbFrames = 10;
    private const int NbChars = 8;

    #endregion

    #region Members

    private readonly Random _random = new();

    #endregion

    #region Props

    public override TimeSpan Interval => TimeSpan.FromMilliseconds(100);

    public override bool IsUnicode => false;

    #endregion

    #region Public methods

    public override IReadOnlyList<string> Frames
    {
        get
        {
            List<string> frames = new();
            var frame = new StringBuilder();

            for (var k = 0; k < NbFrames; ++k)
            {
                for (var i = 0; i < NbChars; ++i)
                {
                    frame.Append(GenerateChar());
                }

                frames.Add(frame.ToString());
                frame.Clear();
            }

            return frames;
        }
    }

    #endregion

    #region Private methods

    private string GenerateChar()
    {
        return (_random.Next(0, 10) % 2 == 0 ? 0 : 1).ToString();
    }

    #endregion
}