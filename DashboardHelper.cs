using System;
using System.Diagnostics;
using System.IO;

// DashboardHelper.exe
// Registered handler for: dashboard://open?path=C:/full/path/to/file&mode=open
//
//   mode=select  -> File Explorer, file highlighted   (default, old behavior)
//   mode=open    -> opens in the default app (Word, Excel, Notepad, ...)
//   mode=word    -> forces Microsoft Word
//   mode=excel   -> forces Microsoft Excel
//   mode=powerpoint / mode=ppt -> forces Microsoft PowerPoint

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

        // Parse the query string ourselves so we decode exactly once
        // (HttpUtility.ParseQueryString already unescapes, and calling
        // Uri.UnescapeDataString on top of it breaks paths with '%').
        string path = GetQueryParam(uri, "path");
        string mode = GetQueryParam(uri, "mode") ?? "select";

        if (string.IsNullOrWhiteSpace(path))
            return 4;

        // Safety: this helper only opens/selects a file path.
        // It does not execute arbitrary commands.
        if (path.IndexOfAny(new[] { '"', '\r', '\n' }) >= 0)
            return 5;

        // The path must actually exist.
        if (!File.Exists(path))
            return 7;

        try
        {
            switch (mode.ToLowerInvariant())
            {
                case "select":
                    // Old behavior: highlight in File Explorer.
                    Process.Start(new ProcessStartInfo
                    {
                        FileName = "explorer.exe",
                        Arguments = "/select,\"" + path + "\"",
                        UseShellExecute = true
                    });
                    break;

                case "word":
                    return LaunchOfficeApp("winword.exe", path);
                case "excel":
                    return LaunchOfficeApp("excel.exe", path);
                case "powerpoint":
                case "ppt":
                    return LaunchOfficeApp("powerpnt.exe", path);

                case "open":
                default:
                    // Open with whatever app Windows associates with this file
                    // (.docx -> Word, .xlsx -> Excel, .pptx -> PowerPoint, ...).
                    Process.Start(new ProcessStartInfo
                    {
                        FileName = path,
                        UseShellExecute = true
                    });
                    break;
            }

            return 0;
        }
        catch
        {
            return 6;
        }
    }

    // Office apps register on PATH when installed, so Process.Start can
    // find winword.exe / excel.exe / powerpnt.exe directly.
    static int LaunchOfficeApp(string appExe, string path)
    {
        try
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = appExe,
                Arguments = "\"" + path + "\"",
                UseShellExecute = true
            });
            return 0;
        }
        catch
        {
            // Office not found or failed to start — fall back to the
            // default app so the file still opens.
            Process.Start(new ProcessStartInfo
            {
                FileName = path,
                UseShellExecute = true
            });
            return 0;
        }
    }

    static string GetQueryParam(Uri uri, string key)
    {
        string query = uri.Query.TrimStart('?');
        if (query.Length == 0) return null;

        foreach (string pair in query.Split('&'))
        {
            int idx = pair.IndexOf('=');
            string k = idx < 0 ? pair : pair.Substring(0, idx);
            string v = idx < 0 ? "" : pair.Substring(idx + 1);

            if (string.Equals(Uri.UnescapeDataString(k), key, StringComparison.OrdinalIgnoreCase))
                return Uri.UnescapeDataString(v.Replace('+', ' '));
        }
        return null;
    }
}
