using System;
using System.Diagnostics;
using System.Web;

class DashboardHelper
{
    [STAThread]
    static int Main(string[] args)
    {
        if (args.Length == 0)
            return 1;

        string uriText = args[0];

        if (!uriText.StartsWith("dashboard://open", StringComparison.OrdinalIgnoreCase))
            return 2;

        if (!Uri.TryCreate(uriText, UriKind.Absolute, out Uri uri))
            return 3;

        string path = HttpUtility.ParseQueryString(uri.Query).Get("path");

        if (string.IsNullOrWhiteSpace(path))
            return 4;

        path = Uri.UnescapeDataString(path);

        // This helper only opens/selects a path in Explorer.
        // It does not execute arbitrary commands.
        if (path.Contains("\"") || path.Contains("\r") || path.Contains("\n"))
            return 5;

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
        catch
        {
            return 6;
        }
    }
}
