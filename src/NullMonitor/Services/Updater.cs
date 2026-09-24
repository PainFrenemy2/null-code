using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Reflection;
using System.Text.Json;

namespace NullMonitor.Services;

public sealed record UpdateInfo(Version Version, string SetupUrl, string PageUrl);

/// <summary>Checks GitHub Releases and installs new versions through the setup exe.</summary>
public static class Updater
{
    public const string RepoUrl = "https://github.com/PainFrenemy2/null-code";
    // The repo hosts several projects (one per branch), so releases are told apart by tag prefix.
    private const string ReleasesApi = "https://api.github.com/repos/PainFrenemy2/null-code/releases?per_page=50";
    public const string TagPrefix = "null-monitor-v";
    public const string ReleasesPage = RepoUrl + "/releases?q=null-monitor&expanded=true";
    private const string SetupAssetName = "NullMonitor-Setup.exe";

    private static readonly HttpClient Http = CreateClient();

    private static HttpClient CreateClient()
    {
        var c = new HttpClient { Timeout = TimeSpan.FromMinutes(5) };
        c.DefaultRequestHeaders.UserAgent.ParseAdd("NullMonitor-Updater");
        c.DefaultRequestHeaders.Accept.ParseAdd("application/vnd.github+json");
        return c;
    }

    public static Version Current
    {
        get
        {
            var v = Assembly.GetExecutingAssembly().GetName().Version ?? new Version(0, 0, 0);
            return new Version(v.Major, v.Minor, Math.Max(0, v.Build));
        }
    }

    /// <summary>The app was installed by the setup (the uninstaller sits next to the exe).</summary>
    public static bool IsInstalled
    {
        get
        {
            var dir = Path.GetDirectoryName(Environment.ProcessPath);
            return dir != null && File.Exists(Path.Combine(dir, "unins000.exe"));
        }
    }

    public static string? UninstallerPath
    {
        get
        {
            var dir = Path.GetDirectoryName(Environment.ProcessPath);
            var path = dir == null ? null : Path.Combine(dir, "unins000.exe");
            return path != null && File.Exists(path) ? path : null;
        }
    }

    /// <summary>Returns info about a newer release, or null if up to date / offline.</summary>
    public static async Task<UpdateInfo?> CheckAsync()
    {
        try
        {
            using var doc = JsonDocument.Parse(await Http.GetStringAsync(ReleasesApi));
            UpdateInfo? best = null;

            foreach (var release in doc.RootElement.EnumerateArray())
            {
                if (release.GetProperty("draft").GetBoolean() || release.GetProperty("prerelease").GetBoolean())
                    continue;

                var tag = release.GetProperty("tag_name").GetString() ?? "";
                if (!tag.StartsWith(TagPrefix, StringComparison.OrdinalIgnoreCase)) continue;
                if (!Version.TryParse(tag[TagPrefix.Length..], out var v)) continue;
                v = new Version(v.Major, v.Minor, Math.Max(0, v.Build));
                if (v <= Current || (best != null && v <= best.Version)) continue;

                string? setup = null;
                foreach (var asset in release.GetProperty("assets").EnumerateArray())
                {
                    if (string.Equals(asset.GetProperty("name").GetString(), SetupAssetName, StringComparison.OrdinalIgnoreCase))
                        setup = asset.GetProperty("browser_download_url").GetString();
                }
                if (setup == null) continue;

                var page = release.TryGetProperty("html_url", out var h) ? h.GetString() : null;
                best = new UpdateInfo(v, setup, page ?? ReleasesPage);
            }
            return best;
        }
        catch
        {
            return null;
        }
    }

    /// <summary>
    /// Downloads the setup and runs it silently after this process exits.
    /// The setup restarts the app when it is done. Caller must quit the app right after.
    /// </summary>
    public static async Task<bool> DownloadAndRunAsync(UpdateInfo info, bool keepAutostart)
    {
        try
        {
            var file = Path.Combine(Path.GetTempPath(), $"NullMonitor-Setup-{info.Version}.exe");
            await using (var src = await Http.GetStreamAsync(info.SetupUrl))
            await using (var dst = File.Create(file))
                await src.CopyToAsync(dst);

            var tasks = keepAutostart ? "autostart" : "!autostart";
            // Wait ~2 s so this process has exited before the setup replaces the exe.
            Process.Start(new ProcessStartInfo("cmd.exe",
                $"/c ping 127.0.0.1 -n 3 >nul & \"{file}\" /VERYSILENT /SUPPRESSMSGBOXES /NORESTART /MERGETASKS=\"{tasks}\"")
            {
                CreateNoWindow = true,
                UseShellExecute = false,
                WindowStyle = ProcessWindowStyle.Hidden,
            });
            return true;
        }
        catch
        {
            return false;
        }
    }

    public static void OpenUrl(string url)
    {
        try { Process.Start(new ProcessStartInfo(url) { UseShellExecute = true }); }
        catch { /* no browser */ }
    }
}
