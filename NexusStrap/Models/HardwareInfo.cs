using System.Runtime.InteropServices;
using Microsoft.Win32;
using NexusStrap.Utility;

namespace NexusStrap.Models
{
    public enum PerformanceTier
    {
        Low,
        Mid,
        High,
        Ultra
    }

    public class HardwareInfo
    {
        public string CpuName { get; set; } = "Unknown";
        public int CpuCoreCount { get; set; }
        public int CpuThreadCount { get; set; }
        public string GpuName { get; set; } = "Unknown";
        public ulong GpuVramBytes { get; set; }
        public string GpuDriverVersion { get; set; } = "";
        public ulong TotalRamBytes { get; set; }
        public string WindowsVersion { get; set; } = "Unknown";
        public long FreeDiskBytes { get; set; }

        public double TotalRamGB => Math.Round(TotalRamBytes / 1024.0 / 1024.0 / 1024.0, 1);
        public double GpuVramGB => Math.Round(GpuVramBytes / 1024.0 / 1024.0 / 1024.0, 1);

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
        private struct MEMORYSTATUSEX
        {
            public uint dwLength;
            public uint dwMemoryLoad;
            public ulong ullTotalPhys;
            public ulong ullAvailPhys;
            public ulong ullTotalPageFile;
            public ulong ullAvailPageFile;
            public ulong ullTotalVirtual;
            public ulong ullAvailVirtual;
            public ulong ullAvailExtendedVirtual;
        }

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool GlobalMemoryStatusEx(ref MEMORYSTATUSEX lpBuffer);

        public static HardwareInfo Detect()
        {
            const string LOG_IDENT = "HardwareInfo::Detect";

            var info = new HardwareInfo();

            try
            {
                info.CpuName = GetRegistryString(
                    @"HARDWARE\DESCRIPTION\System\CentralProcessor\0",
                    "ProcessorNameString",
                    "Unknown CPU");

                info.CpuCoreCount = Environment.ProcessorCount;
                info.CpuThreadCount = Environment.ProcessorCount;
            }
            catch (Exception ex)
            {
                App.Logger.WriteException(LOG_IDENT, ex);
            }

            try
            {
                using var gpuKey = Registry.LocalMachine.OpenSubKey(@"SYSTEM\CurrentControlSet\Control\Class\{4d36e968-e325-11ce-bfc1-08002be10318}");

                // the first adapter (0000) is usually the primary/active GPU
                using var adapterKey = gpuKey?.OpenSubKey("0000");

                if (adapterKey?.GetValue("DriverDesc") is string gpuName)
                {
                    info.GpuName = gpuName.Trim();
                    info.GpuDriverVersion = adapterKey.GetValue("DriverVersion") as string ?? "";
                }

                if (adapterKey?.GetValue("HardwareInformation.qwMemorySize") is byte[] vramBytes)
                    info.GpuVramBytes = BitConverter.ToUInt64(vramBytes, 0);
            }
            catch (Exception ex)
            {
                App.Logger.WriteException(LOG_IDENT, ex);
            }

            try
            {
                var memoryStatus = new MEMORYSTATUSEX { dwLength = (uint)Marshal.SizeOf<MEMORYSTATUSEX>() };

                if (GlobalMemoryStatusEx(ref memoryStatus))
                    info.TotalRamBytes = memoryStatus.ullTotalPhys;
            }
            catch (Exception ex)
            {
                App.Logger.WriteException(LOG_IDENT, ex);
            }

            try
            {
                string productName = GetRegistryString(
                    @"SOFTWARE\Microsoft\Windows NT\CurrentVersion",
                    "ProductName",
                    "Windows");

                string displayVersion = GetRegistryString(
                    @"SOFTWARE\Microsoft\Windows NT\CurrentVersion",
                    "DisplayVersion",
                    "");

                string build = GetRegistryString(
                    @"SOFTWARE\Microsoft\Windows NT\CurrentVersion",
                    "CurrentBuildNumber",
                    "");

                info.WindowsVersion = displayVersion.Length > 0
                    ? $"{productName} {displayVersion} (Build {build})"
                    : $"{productName} (Build {build})";
            }
            catch (Exception ex)
            {
                App.Logger.WriteException(LOG_IDENT, ex);
            }

            try
            {
                info.FreeDiskBytes = Filesystem.GetFreeDiskSpace(Paths.Base);
            }
            catch
            {
                info.FreeDiskBytes = -1;
            }

            return info;
        }

        private static string GetRegistryString(string subKey, string valueName, string fallback)
        {
            using var key = Registry.LocalMachine.OpenSubKey(subKey);
            return key?.GetValue(valueName) as string ?? fallback;
        }

        /// <summary>
        /// Scores the GPU from 0 (very weak) to 4 (high end) based on the
        /// known GPU family in the name, falling back to VRAM size.
        /// </summary>
        public int GpuScore()
        {
            string name = GpuName.ToLowerInvariant();

            if (name.Contains("rtx 50") || name.Contains("rtx 40") || name.Contains("rx 7900") || name.Contains("rx 7800"))
                return 4;

            if (name.Contains("rtx 30") || name.Contains("rtx 20") || name.Contains("gtx 16") || name.Contains("rx 6000") || name.Contains("rx 7000") || name.Contains("arc a7") || name.Contains("arc a5"))
                return 3;

            if (name.Contains("gtx 10") || name.Contains("gtx 9") || name.Contains("rx 5000") || name.Contains("rx 500") || name.Contains("radeon rx 580") || name.Contains("radeon rx 570"))
                return 2;

            if (name.Contains("gtx 7") || name.Contains("gtx 6") || name.Contains("intel hd") || name.Contains("intel uhd") || name.Contains("intel iris") || name.Contains("radeon graphics") || name.Contains("radeon r5") || name.Contains("radeon r7"))
                return 1;

            if (name.Contains("microsoft basic") || name.Contains("basic display"))
                return 0;

            // fallback: score based on VRAM
            if (GpuVramBytes >= 8UL * 1024 * 1024 * 1024)
                return 3;

            if (GpuVramBytes >= 4UL * 1024 * 1024 * 1024)
                return 2;

            if (GpuVramBytes >= 2UL * 1024 * 1024 * 1024)
                return 1;

            return 0;
        }

        /// <summary>
        /// Scores the CPU from 0 (old/weak) to 3. Uses core count and,
        /// when reasonably detectable, the processor generation.
        /// </summary>
        public int CpuScore()
        {
            string name = CpuName.ToLowerInvariant();

            int generation = DetectCpuGeneration(name);

            int score = 1;

            if (CpuCoreCount >= 16)
                score = 3;
            else if (CpuCoreCount >= 8)
                score = 2;
            else if (CpuCoreCount >= 4)
                score = 1;
            else
                score = 0;

            // modern Intel (12th gen+) and AMD (Ryzen 7000+/Zen 4+) get a bump
            if ((name.Contains("intel") && generation >= 12) || (name.Contains("ryzen") && generation >= 7000))
                score += 1;

            // very old CPUs (Intel <= 7th gen, AMD pre-Zen) get a penalty
            if ((name.Contains("intel") && generation > 0 && generation <= 7) || (name.Contains("core 2") || name.Contains("phenom") || name.Contains("fx-") || name.Contains("athlon")))
                score -= 1;

            return Math.Clamp(score, 0, 3);
        }

        private static int DetectCpuGeneration(string lowerName)
        {
            try
            {
                if (lowerName.Contains("ryzen"))
                {
                    // e.g. "Ryzen 5 7600X" -> 7000
                    var match = System.Text.RegularExpressions.Regex.Match(lowerName, @"ryzen\s*\d\s+(\d{4})");
                    if (match.Success && int.TryParse(match.Groups[1].Value, out int ryzenGen))
                        return ryzenGen;
                }
                else if (lowerName.Contains("core i"))
                {
                    // e.g. "Intel(R) Core(TM) i5-13600K" -> 13, "i7-6700K" -> 6
                    var match = System.Text.RegularExpressions.Regex.Match(lowerName, @"i[3579][- ](\d{2})");
                    if (match.Success && int.TryParse(match.Groups[1].Value, out int intelGen))
                        return intelGen;
                }
            }
            catch
            {
            }

            return 0; // unknown generation
        }

        /// <summary>
        /// Combines the hardware scores into an overall performance tier.
        /// </summary>
        public PerformanceTier GetTier()
        {
            int total = GpuScore() + CpuScore();

            if (TotalRamGB >= 16)
                total += 1;
            else if (TotalRamGB <= 6)
                total -= 1;

            if (total >= 8)
                return PerformanceTier.Ultra;

            if (total >= 6)
                return PerformanceTier.High;

            if (total >= 4)
                return PerformanceTier.Mid;

            return PerformanceTier.Low;
        }

        public string GetTierName() => GetTier() switch
        {
            PerformanceTier.Ultra => "Ultra",
            PerformanceTier.High => "High",
            PerformanceTier.Mid => "Mid",
            _ => "Low"
        };
    }
}