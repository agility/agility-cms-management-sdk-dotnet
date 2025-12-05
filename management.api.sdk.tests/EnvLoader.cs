using System;
using System.IO;

namespace agility.utils
{
    /// <summary>
    /// Simple .env file loader for test configuration
    /// </summary>
    public static class EnvLoader
    {
        /// <summary>
        /// Loads environment variables from a .env file
        /// </summary>
        /// <param name="filePath">Path to the .env file (defaults to .env in current directory)</param>
        public static void Load(string filePath = ".env")
        {
            if (!File.Exists(filePath))
            {
                // If .env doesn't exist, try looking in the test project directory
                var testProjectPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, filePath);
                if (!File.Exists(testProjectPath))
                {
                    Console.WriteLine($"Warning: .env file not found at {filePath} or {testProjectPath}");
                    return;
                }
                filePath = testProjectPath;
            }

            foreach (var line in File.ReadAllLines(filePath))
            {
                // Skip empty lines and comments
                if (string.IsNullOrWhiteSpace(line) || line.TrimStart().StartsWith("#"))
                    continue;

                var parts = line.Split('=', 2);
                if (parts.Length != 2)
                    continue;

                var key = parts[0].Trim();
                var value = parts[1].Trim();

                // Only set if not already set (environment variables take precedence)
                if (string.IsNullOrEmpty(Environment.GetEnvironmentVariable(key)))
                {
                    Environment.SetEnvironmentVariable(key, value);
                }
            }
        }
    }
}
