using LibreHardwareMonitor.Hardware;

namespace NullMonitor.Services;

/// <summary>Polls LibreHardwareMonitor on a background thread and publishes snapshots.</summary>
public sealed class HardwareService : IDisposable
{
    private readonly Computer _computer = new()
    {
        IsCpuEnabled = true,
        IsGpuEnabled = true,
        IsMemoryEnabled = true,
        IsMotherboardEnabled = true,
        IsStorageEnabled = true,
        IsNetworkEnabled = true,
    };

    private readonly CancellationTokenSource _cts = new();
    private readonly object _lock = new();
    private Task? _loop;

    public event Action<Snapshot>? Updated;
    public event Action<string>? Failed;

    public TimeSpan Interval { get; set; } = TimeSpan.FromSeconds(1);

    public void Start()
    {
        _loop = Task.Run(async () =>
        {
            try
            {
                _computer.Open();
            }
            catch (Exception ex)
            {
                Failed?.Invoke(ex.Message);
                return;
            }

            while (!_cts.IsCancellationRequested)
            {
                try
                {
                    Snapshot snap;
                    lock (_lock)
                    {
                        foreach (var hw in _computer.Hardware)
                            UpdateRecursive(hw);
                        snap = Build();
                    }
                    Updated?.Invoke(snap);
                }
                catch (Exception ex)
                {
                    Failed?.Invoke(ex.Message);
                }

                try { await Task.Delay(Interval, _cts.Token); }
                catch (TaskCanceledException) { break; }
            }
        });
    }

    private static void UpdateRecursive(IHardware hw)
    {
        hw.Update();
        foreach (var sub in hw.SubHardware)
            UpdateRecursive(sub);
    }

    private static IEnumerable<ISensor> AllSensors(IHardware hw) =>
        hw.Sensors.Concat(hw.SubHardware.SelectMany(AllSensors));

    private static float? Find(IHardware hw, SensorType type, params string[] names)
    {
        var sensors = AllSensors(hw).Where(s => s.SensorType == type && s.Value.HasValue).ToList();
        foreach (var name in names)
        {
            var s = sensors.FirstOrDefault(x => x.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
            if (s != null) return s.Value;
        }
        return null;
    }

    private static float? FindContains(IHardware hw, SensorType type, string part) =>
        AllSensors(hw).FirstOrDefault(s => s.SensorType == type && s.Value.HasValue &&
                                           s.Name.Contains(part, StringComparison.OrdinalIgnoreCase))?.Value;

    private Snapshot Build()
    {
        var snap = new Snapshot();
        var hardware = _computer.Hardware.ToList();

        // CPU
        var cpu = hardware.FirstOrDefault(h => h.HardwareType == HardwareType.Cpu);
        if (cpu != null)
        {
            snap.CpuName = cpu.Name;
            snap.CpuLoad = Find(cpu, SensorType.Load, "CPU Total");
            snap.CpuTemp = Find(cpu, SensorType.Temperature,
                               "CPU Package", "Core (Tctl/Tdie)", "Core (Tdie)", "Core (Tctl)", "Core Max", "Core Average", "CPU Cores")
                           ?? AllSensors(cpu).Where(s => s.SensorType == SensorType.Temperature && s.Value > 0)
                                             .Select(s => s.Value).Max();
            snap.CpuPower = Find(cpu, SensorType.Power, "CPU Package", "Package");

            var clocks = AllSensors(cpu)
                .Where(s => s.SensorType == SensorType.Clock && s.Value > 0 &&
                            s.Name.StartsWith("Core #", StringComparison.OrdinalIgnoreCase))
                .Select(s => s.Value!.Value).ToList();
            if (clocks.Count > 0) snap.CpuClock = clocks.Average();

            var cores = AllSensors(cpu)
                .Where(s => s.SensorType == SensorType.Load && s.Value.HasValue &&
                            s.Name.StartsWith("CPU Core #", StringComparison.OrdinalIgnoreCase))
                .ToList();
            // Prefer per-thread sensors when present, otherwise per-core.
            var threads = cores.Where(s => s.Name.Contains("Thread", StringComparison.OrdinalIgnoreCase)).ToList();
            snap.CoreLoads.AddRange((threads.Count > 0 ? threads : cores).Select(s => s.Value!.Value));
        }

        // GPU: prefer a discrete card
        var gpu = hardware.FirstOrDefault(h => h.HardwareType == HardwareType.GpuNvidia)
               ?? hardware.FirstOrDefault(h => h.HardwareType == HardwareType.GpuAmd)
               ?? hardware.FirstOrDefault(h => h.HardwareType == HardwareType.GpuIntel);
        if (gpu != null)
        {
            snap.GpuName = gpu.Name;
            snap.GpuLoad = Find(gpu, SensorType.Load, "GPU Core", "D3D 3D");
            snap.GpuTemp = Find(gpu, SensorType.Temperature, "GPU Core", "GPU Package");
            snap.GpuHotSpot = Find(gpu, SensorType.Temperature, "GPU Hot Spot");
            snap.GpuClock = Find(gpu, SensorType.Clock, "GPU Core");
            snap.GpuPower = Find(gpu, SensorType.Power, "GPU Package", "GPU Power", "GPU Core");
            snap.GpuMemUsed = Find(gpu, SensorType.SmallData, "GPU Memory Used", "D3D Dedicated Memory Used");
            snap.GpuMemTotal = Find(gpu, SensorType.SmallData, "GPU Memory Total", "D3D Dedicated Memory Total");
            snap.GpuFan = FindContains(gpu, SensorType.Fan, "Fan");
        }

        // RAM
        var mem = hardware.FirstOrDefault(h => h.HardwareType == HardwareType.Memory &&
                                               !h.Name.Contains("Virtual", StringComparison.OrdinalIgnoreCase))
               ?? hardware.FirstOrDefault(h => h.HardwareType == HardwareType.Memory);
        if (mem != null)
        {
            snap.RamLoad = Find(mem, SensorType.Load, "Memory", "Physical Memory");
            snap.RamUsed = Find(mem, SensorType.Data, "Memory Used", "Physical Memory Used");
            snap.RamAvailable = Find(mem, SensorType.Data, "Memory Available", "Physical Memory Available");
        }

        // Motherboard + fans
        var board = hardware.FirstOrDefault(h => h.HardwareType == HardwareType.Motherboard);
        if (board != null)
        {
            snap.BoardName = board.Name;
            foreach (var s in AllSensors(board).Where(s => s.SensorType == SensorType.Fan && s.Value > 0))
                snap.Fans.Add(new FanStat { Name = s.Name, Rpm = s.Value!.Value });
        }

        // Storage
        foreach (var disk in hardware.Where(h => h.HardwareType == HardwareType.Storage))
        {
            snap.Disks.Add(new DiskStat
            {
                Name = disk.Name,
                Temperature = AllSensors(disk).FirstOrDefault(s => s.SensorType == SensorType.Temperature && s.Value > 0)?.Value,
                UsedPercent = Find(disk, SensorType.Load, "Used Space"),
                ReadRate = Find(disk, SensorType.Throughput, "Read Rate"),
                WriteRate = Find(disk, SensorType.Throughput, "Write Rate"),
            });
        }

        // Network: sum all adapters
        foreach (var nic in hardware.Where(h => h.HardwareType == HardwareType.Network))
        {
            snap.NetUp += Find(nic, SensorType.Throughput, "Upload Speed") ?? 0;
            snap.NetDown += Find(nic, SensorType.Throughput, "Download Speed") ?? 0;
        }

        return snap;
    }

    private bool _disposed;

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;
        _cts.Cancel();
        try { _loop?.Wait(2000); } catch { /* ignore */ }
        lock (_lock)
        {
            try { _computer.Close(); } catch { /* ignore */ }
        }
    }
}
