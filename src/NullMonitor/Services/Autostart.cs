using System.Diagnostics;
using System.IO;
using System.Security;
using System.Security.Principal;
using System.Text;

namespace NullMonitor.Services;

/// <summary>
/// Autostart through Task Scheduler: the app needs admin rights, and the Run registry key
/// can't start elevated programs without a UAC prompt.
/// </summary>
public static class Autostart
{
    private const string TaskName = "NullMonitor";

    /// <summary>
    /// The portable exe autostarts from a private copy, so the downloaded file can be deleted or moved.
    /// An installed app autostarts from its install folder.
    /// </summary>
    public static readonly string InstallDir =
        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "NullMonitor");

    public static string InstalledExe => Path.Combine(InstallDir, "NullMonitor.exe");

    private static bool IsRunningInstalledCopy() =>
        string.Equals(Path.GetFullPath(Environment.ProcessPath ?? ""), Path.GetFullPath(InstalledExe),
            StringComparison.OrdinalIgnoreCase);

    public static bool IsEnabled() => Schtasks($"/Query /TN \"{TaskName}\"") == 0;

    public static bool Enable()
    {
        var exe = Environment.ProcessPath;
        if (string.IsNullOrEmpty(exe)) return false;

        if (Updater.IsInstalled)
        {
            // Installed by the setup: run straight from Program Files.
        }
        else if (!IsRunningInstalledCopy())
        {
            try
            {
                Directory.CreateDirectory(InstallDir);
                File.Copy(exe, InstalledExe, overwrite: true);
                exe = InstalledExe;
            }
            catch
            {
                // Copy failed (disk, permissions) — autostart the current file instead.
            }
        }
        else
        {
            exe = InstalledExe;
        }

        var user = WindowsIdentity.GetCurrent().Name;
        var xml = $"""
            <?xml version="1.0" encoding="UTF-16"?>
            <Task version="1.2" xmlns="http://schemas.microsoft.com/windows/2004/02/mit/task">
              <RegistrationInfo>
                <Author>painfrenemy</Author>
                <Description>Null Monitor — запуск вместе с Windows</Description>
              </RegistrationInfo>
              <Triggers>
                <LogonTrigger>
                  <Enabled>true</Enabled>
                  <UserId>{SecurityElement.Escape(user)}</UserId>
                  <Delay>PT5S</Delay>
                </LogonTrigger>
              </Triggers>
              <Principals>
                <Principal id="Author">
                  <UserId>{SecurityElement.Escape(user)}</UserId>
                  <LogonType>InteractiveToken</LogonType>
                  <RunLevel>HighestAvailable</RunLevel>
                </Principal>
              </Principals>
              <Settings>
                <MultipleInstancesPolicy>IgnoreNew</MultipleInstancesPolicy>
                <DisallowStartIfOnBatteries>false</DisallowStartIfOnBatteries>
                <StopIfGoingOnBatteries>false</StopIfGoingOnBatteries>
                <ExecutionTimeLimit>PT0S</ExecutionTimeLimit>
                <AllowHardTerminate>true</AllowHardTerminate>
                <Priority>5</Priority>
              </Settings>
              <Actions Context="Author">
                <Exec>
                  <Command>{SecurityElement.Escape(exe)}</Command>
                  <Arguments>--autostart</Arguments>
                </Exec>
              </Actions>
            </Task>
            """;

        var tmp = Path.Combine(Path.GetTempPath(), "NullMonitor-task.xml");
        try
        {
            File.WriteAllText(tmp, xml, Encoding.Unicode);
            return Schtasks($"/Create /TN \"{TaskName}\" /XML \"{tmp}\" /F") == 0;
        }
        finally
        {
            try { File.Delete(tmp); } catch { /* ignore */ }
        }
    }

    public static bool Disable()
    {
        bool ok = Schtasks($"/Delete /TN \"{TaskName}\" /F") == 0;
        if (ok && !Updater.IsInstalled && !IsRunningInstalledCopy())
        {
            try { File.Delete(InstalledExe); } catch { /* ignore */ }
        }
        return ok;
    }

    private static int Schtasks(string args)
    {
        try
        {
            using var p = Process.Start(new ProcessStartInfo("schtasks.exe", args)
            {
                CreateNoWindow = true,
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
            });
            if (p == null) return -1;
            p.StandardOutput.ReadToEnd();
            p.StandardError.ReadToEnd();
            p.WaitForExit(10_000);
            return p.ExitCode;
        }
        catch
        {
            return -1;
        }
    }
}
