namespace NullMonitor.Services;

public sealed class DiskStat
{
    public string Name { get; init; } = "";
    public float? Temperature { get; init; }
    public float? UsedPercent { get; init; }
    public float? ReadRate { get; init; }
    public float? WriteRate { get; init; }
}

public sealed class FanStat
{
    public string Name { get; init; } = "";
    public float Rpm { get; init; }
}

public sealed class Snapshot
{
    public string CpuName { get; set; } = "CPU";
    public float? CpuLoad { get; set; }
    public float? CpuTemp { get; set; }
    /// <summary>CpuTemp comes from the ACPI thermal zone, not the CPU itself.</summary>
    public bool CpuTempIsApprox { get; set; }
    public float? CpuClock { get; set; }
    public float? CpuPower { get; set; }
    public List<float> CoreLoads { get; } = new();

    public string GpuName { get; set; } = "GPU";
    public float? GpuLoad { get; set; }
    public float? GpuTemp { get; set; }
    public float? GpuHotSpot { get; set; }
    public float? GpuClock { get; set; }
    public float? GpuPower { get; set; }
    public float? GpuMemUsed { get; set; }
    public float? GpuMemTotal { get; set; }
    public float? GpuFan { get; set; }

    public float? RamLoad { get; set; }
    public float? RamUsed { get; set; }
    public float? RamAvailable { get; set; }

    public string BoardName { get; set; } = "";
    public List<DiskStat> Disks { get; } = new();
    public List<FanStat> Fans { get; } = new();

    public float NetUp { get; set; }
    public float NetDown { get; set; }
}
