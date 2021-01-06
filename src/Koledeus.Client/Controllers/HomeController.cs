using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Koledeus.Client.Models;

namespace Koledeus.Client.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        public IActionResult Index()
        {
            var cpuUseage = GetCpuUsageForProcess();
            return View(cpuUseage);
        }


        private double GetCpuUsageForProcess()
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

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel {RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier});
        }
    }
}