using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace Koledeus.Client.Services
{
    public interface ICPUInfoService
    {
        (double, long) GetUsageOfProcess();
    }

    public class ProcessInfoService : ICPUInfoService
    {
        public (double, long) GetUsageOfProcess()
        {
            double totalCpuUsage = 0;
            long totalMemoryUsage = 0;
            var processes = Process.GetProcesses();

            // TODO (peacecwz): This code is not working on threadsafe, It needs refactoring for working threadsafe
            Parallel.ForEach(processes, process =>
            {
                var cpuUsage = GetCPUUseageByProcess(process);
                var memoryUsage = GetMemoryUsageByProcess(process);
                totalCpuUsage += cpuUsage;
                totalMemoryUsage += memoryUsage;
            });

            return (totalCpuUsage, totalMemoryUsage);
        }

        private long GetMemoryUsageByProcess(Process process)
        {
            return process.WorkingSet64;
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