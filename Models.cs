using System;
using System.Collections.Generic;

namespace AddToAutorun
{
    public enum AutorunHive
    {
        CurrentUser,
        LocalMachine
    }

    public record AutorunEntry(string Name, string FullPath, AutorunHive Hive);

    public static class FileConstants
    {
        public const string AutorunRegKey = @"Software\Microsoft\Windows\CurrentVersion\Run";

        public static readonly HashSet<string> SupportedExtensions = new(StringComparer.OrdinalIgnoreCase)
        {
            ".exe"
        };
    }
}


