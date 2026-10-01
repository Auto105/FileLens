using System.Diagnostics;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace FileLens.IntegrationTests.Fixtures;

internal static class TestEnvironment
{
    public static void CreateSymbolicLink(string path, string target, bool directory)
    {
        try
        {
            if (directory)
            {
                Directory.CreateSymbolicLink(path, target);
            }
            else
            {
                File.CreateSymbolicLink(path, target);
            }
        }
        catch (Exception exception) when (exception is UnauthorizedAccessException or IOException or PlatformNotSupportedException)
        {
            Assert.Inconclusive($"Symbolic link fixture is unavailable: {exception.GetType().Name}: {exception.Message}");
        }
    }

    public static void CreateJunction(string path, string target)
    {
        // These arguments are test-owned paths. Refuse shell metacharacters rather than interpret them.
        if (path.IndexOfAny(['"', '%', '!', '&', '|', '<', '>', '^', '\r', '\n']) >= 0
            || target.IndexOfAny(['"', '%', '!', '&', '|', '<', '>', '^', '\r', '\n']) >= 0)
        {
            Assert.Inconclusive("Junction fixture paths contain unsupported command-shell characters.");
        }

        var startInfo = new ProcessStartInfo(Path.Combine(Environment.SystemDirectory, "cmd.exe"))
        {
            Arguments = $"/d /c mklink /J \"{path}\" \"{target}\"",
            UseShellExecute = false,
            CreateNoWindow = true,
            RedirectStandardOutput = true,
            RedirectStandardError = true
        };
        using var process = Process.Start(startInfo)
            ?? throw new IOException("Could not start the junction fixture helper.");
        var output = process.StandardOutput.ReadToEndAsync();
        var error = process.StandardError.ReadToEndAsync();
        if (!process.WaitForExit(10_000))
        {
            process.Kill(true);
            process.WaitForExit();
            throw new TimeoutException("Junction fixture creation did not finish.");
        }

        var detail = output.GetAwaiter().GetResult() + error.GetAwaiter().GetResult();
        if (process.ExitCode != 0)
        {
            Assert.Inconclusive($"Junction fixture is unavailable (exit {process.ExitCode}): {detail}");
        }

        Assert.IsTrue((File.GetAttributes(path) & FileAttributes.ReparsePoint) != 0);
    }
}
