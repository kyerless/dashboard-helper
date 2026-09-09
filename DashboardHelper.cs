using System;
using System.Diagnostics;
using System.IO;

class DashboardHelper
{
    [STAThread]
    static int Main(string[] args)
    {
        if (args.Length == 0)
            return 1;

        string uriText = args[0];

        if (!uriText.StartsWith(
            "dashboard://open",
            StringComparison.OrdinalIgnoreCase))
        {
            return 2;
        }

        if (!Uri.TryCreate(
            uriText,
            UriKind.Absolute,
            out Uri? uri))
        {
            return 3;
        }

        string path = GetQueryParameter(uri, "path");

        if (string.IsNullOrWhiteSpace(path))
            return 4;

        // Convert URL-style slashes to Windows slashes.
        path = path.Replace('/', '\\');

        // Basic safety check.
        if (path.Contains("\"") ||
            path.Contains("\r") ||
            path.Contains("\n"))
        {
            return 5;
        }

        // Make sure the target actually exists.
        if (!File.Exists(path) && !Directory.Exists(path))
        {
            MessageBox(
                "Dashboard Helper\n\n" +
                "The requested file could not be found:\n\n" +
                path +
                "\n\n" +
                "Check the Recent Files Folder path in Dashboard Settings."
            );

            return 6;
        }

        try
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = "explorer.exe",
                Arguments = "/select,\"" + path + "\"",
                UseShellExecute = true
            });

            return 0;
        }
        catch (Exception ex)
        {
            MessageBox(
                "Dashboard Helper\n\n" +
                "Could not open File Explorer.\n\n" +
                ex.Message
            );

            return 7;
        }
    }

    static string GetQueryParameter(Uri uri, string key)
    {
        string query = uri.Query.TrimStart('?');

        foreach (string part in query.Split(
            '&',
            StringSplitOptions.RemoveEmptyEntries))
        {
            string[] pieces = part.Split('=', 2);

            if (pieces.Length != 2)
                continue;

            string name = Uri.UnescapeDataString(
                pieces[0]);

            if (!string.Equals(
                name,
                key,
                StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            string value = Uri.UnescapeDataString(
                pieces[1].Replace("+", " "));

            return value;
        }

        return "";
    }

    static void MessageBox(string message)
    {
        Process.Start(new ProcessStartInfo
        {
            FileName = "msg.exe",
            Arguments = "\"" + message.Replace("\"", "'") + "\"",
            UseShellExecute = true,
            CreateNoWindow = true
        });
    }
}
