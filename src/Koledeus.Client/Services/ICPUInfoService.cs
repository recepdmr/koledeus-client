using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace Koledeus.Client.Services
{
    public interface ICPUInfoService
    {
        double GetCpuUsageForProcess();
    }

    public class CPUInfoService : ICPUInfoService
    {
        public double GetCpuUsageForProcess()
        {
            double total = 0;
            var processes = Process.GetProcesses();

            foreach (var process in processes)
            {
                var cpuUseage = GetCPUUseageByProcess(process);

                total += cpuUseage;
            }

            return total;
        }

        private double GetCPUUseageByProcess(Process process)
        {
            var startTime = DateTime.UtcNow;
            var startCpuUsage = process.TotalProcessorTime;

            var endTime = DateTime.UtcNow;
            var endCpuUsage = process.TotalProcessorTime;
            var cpuUsedMs = (endCpuUsage - startCpuUsage).TotalMilliseconds;
            var totalMsPassed = (endTime - startTime).TotalMilliseconds;
            var cpuUsageTotal = cpuUsedMs / (Environment.ProcessorCount * totalMsPassed);
            return cpuUsageTotal * 100;
        }
    }
}