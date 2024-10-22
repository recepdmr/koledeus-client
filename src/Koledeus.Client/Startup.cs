using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using ElectronNET.API;
using ElectronNET.API.Entities;
using Grpc.Net.Client;
using Koledeus.Client.Services;
using Koledeus.Contract;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Koledeus.Client
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddSingleton(provider =>
            {
                return new Cpu.CpuClient(
                    GrpcChannel.ForAddress(Configuration.GetValue<string>("CPUTracker:ServerUrl"),
                        new GrpcChannelOptions()
                        {
                            HttpClient = new HttpClient(new HttpClientHandler())
                        }));
            });
            services.AddSingleton<ICPUInfoService, CPUInfoService>();
            services.AddHostedService<CPUTrackerHostedService>();
            services.AddControllersWithViews();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, IHostApplicationLifetime lifetime)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthorization();


            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Home}/{action=Index}/{id?}");
            });

            Task.Run(() => BootstrapElectron(lifetime));
        }
        
        private async Task BootstrapElectron(IHostApplicationLifetime lifetime)
        {
            var browserWindow = await Electron.WindowManager.CreateWindowAsync(new BrowserWindowOptions()
            {
                Show = false
            });

            browserWindow.OnReadyToShow += () =>
            {
                browserWindow.Show();
            };

            browserWindow.OnClosed += lifetime.StopApplication;
        }
    }
}