﻿using System.Reflection;

namespace Horus.Shared.Helpers;

public static class VersionHelper
{
    #region Public methods

    public static Version GetVersion()
    {
        return Parse(
            Assembly.GetExecutingAssembly()
                .GetName()
                .Version!
                .ToString()
        );
    }

    public static Version Parse(string version)
    {
        var parts = version.Split('.', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries)
            .Take(3)
            .Select(int.Parse)
            .ToList();
        return new Version(parts[0], parts[1], parts[2]);
    }

    #endregion
}