using System.IO;
using System.Text.Json;

namespace NullMonitor.Services;

public sealed class AppSettings
{
    public bool ShowCpuLoad { get; set; } = true;
    public bool ShowCpuTemp { get; set; } = true;
    public bool ShowGpuLoad { get; set; } = true;
    public bool ShowGpuTemp { get; set; } = true;
    public bool ShowRam { get; set; } = true;
    public bool ShowNetDown { get; set; } = true;
    public bool ShowNetUp { get; set; } = true;

    /// <summary>1 or 2 lines of text on the taskbar.</summary>
    public int Rows { get; set; } = 2;
    public double FontSize { get; set; } = 12;
    public bool ColorTemps { get; set; } = true;
    /// <summary>auto | light | dark — colour of the taskbar text.</summary>
    public string Theme { get; set; } = "auto";

    public double IntervalSeconds { get; set; } = 1;

    /// <summary>tray = next to the tray icons, left = left edge of the taskbar.</summary>
    public string Anchor { get; set; } = "tray";
    /// <summary>Manual horizontal shift in physical pixels (set by dragging).</summary>
    public int OffsetPx { get; set; }

    public bool HideInFullscreen { get; set; } = true;

    /// <summary>Monitor device name (\\.\DISPLAY2) whose taskbar shows the widget; empty = main taskbar.</summary>
    public string MonitorDevice { get; set; } = "";

    /// <summary>Download and install new releases automatically (installed version only).</summary>
    public bool AutoUpdate { get; set; } = true;

    [System.Text.Json.Serialization.JsonIgnore]
    public bool IsFirstRun { get; private set; }

    private static readonly string Dir =
        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "NullMonitor");
    private static readonly string FilePath = Path.Combine(Dir, "settings.json");
    private static readonly JsonSerializerOptions Json = new() { WriteIndented = true };

    public static AppSettings Load()
    {
        try
        {
            if (File.Exists(FilePath))
                return JsonSerializer.Deserialize<AppSettings>(File.ReadAllText(FilePath), Json) ?? new AppSettings();
        }
        catch { /* corrupted file — fall back to defaults */ }

        return new AppSettings { IsFirstRun = true };
    }

    public void Save()
    {
        try
        {
            Directory.CreateDirectory(Dir);
            File.WriteAllText(FilePath, JsonSerializer.Serialize(this, Json));
        }
        catch { /* not critical */ }
    }
}
