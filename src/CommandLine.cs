﻿namespace Ubiquiti.UniFi
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// The command line class.
    /// </summary>
    public static class CommandLine
    {
        /// <summary>
        /// Parses the provided command line parameters.
        /// </summary>
        /// <param name="prefixes"></param>
        /// <param name="args">The command line parameters to parse.</param>
        /// <returns>Returns a dictionary of key value pairs containing the parsed command line parameters.</returns>
        public static Dictionary<string, object> ParseArgs(string[] prefixes, string[] args)
        {
            var dict = new Dictionary<string, object>();
            for (int i = 0; i < args.Length; i++)
            {
                var prefix = GetPrefix(prefixes, args[i]);
                if (!string.IsNullOrEmpty(prefix))
                {
                    try
                    {
                        var key = args[i].Substring(prefix.Length, args[i].Length - prefix.Length);
                        var isEnd = args.Length - 1 == i;
                        var isCommand = !isEnd && args[i + 1].Contains(prefix);
                        if (isCommand || isEnd)
                        {
                            dict.Add(key, true); //TODO: Provide better solution.
                            continue;
                        }

                        var value = args[i + 1];
                        dict.Add(key, value);
                    }
                    catch (Exception ex)
                    {
                        Console.Error.WriteLine($"Failed to parse argument: {ex}");
                    }
                }
            }

            return dict;
        }

        private static string GetPrefix(string[] prefixes, string argument)
        {
            foreach (var prefix in prefixes)
            {
                var arg = argument.Substring(0, prefix.Length);
                if (arg == prefix)
                {
                    return prefix;
                }
            }

            return null;
        }
    }
}