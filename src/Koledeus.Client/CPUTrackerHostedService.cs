using System;
using System.Threading;
using System.Threading.Tasks;
using Grpc.Core;
using Koledeus.Client.Services;
using Koledeus.Contract;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Koledeus.Client
{
    public class CPUTrackerHostedService : IHostedService, IDisposable
    {
        private readonly ILogger<CPUTrackerHostedService> _logger;
        private readonly IConfiguration _configuration;
        private readonly ICPUInfoService _cpuInfoService;
        private readonly Cpu.CpuClient _cpuClient;
        private Timer _timer;
        private AsyncClientStreamingCall<CPUInfoRequest, CPUInfoReply> _feedCpuInfoCall;

        public CPUTrackerHostedService(ILogger<CPUTrackerHostedService> logger, IConfiguration configuration,
            ICPUInfoService cpuInfoService, Cpu.CpuClient cpuClient)
        {
            _logger = logger;
            _configuration = configuration;
            _cpuInfoService = cpuInfoService;
            _cpuClient = cpuClient;
        }

        public Task StartAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("CPU Tracker hosted service started");
            _feedCpuInfoCall = _cpuClient.FeedCPUInfo(new Metadata());
            _timer = new Timer(DoWork, null, TimeSpan.Zero,
                TimeSpan.FromSeconds(_configuration.GetValue<double>("CPUTracker:Period")));

            return Task.CompletedTask;
        }

        private async void DoWork(object state)
        {
            var (cpuPercentage, memoryUsage) = _cpuInfoService.GetUsageOfProcess();

            await _feedCpuInfoCall.RequestStream.WriteAsync(new CPUInfoRequest()
            {
                CpuPercentage = cpuPercentage,
                MemoryUsage = memoryUsage
            });

            _logger.LogInformation($"CPU Useage: {cpuPercentage}%");
        }

        public async Task StopAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Timed Hosted Service is stopping.");
            await _feedCpuInfoCall.RequestStream.CompleteAsync();

            var response = await _feedCpuInfoCall.ResponseAsync;

            _logger.LogInformation($"Server Response is {response.IsSuccess}");

            _timer?.Change(Timeout.Infinite, 0);
        }

        public void Dispose()
        {
            _feedCpuInfoCall?.Dispose();
            _timer?.Dispose();
        }
    }
}