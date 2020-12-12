using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Risk.Shared;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Hosting.Server.Features;
using System.Net.Http;
using Westwind.AspNetCore.LiveReload;

namespace WyattClient
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
            services.AddControllers();
            services.AddHttpClient();
            services.AddLiveReload();
            services.AddRazorPages().AddRazorRuntimeCompilation();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, IHttpClientFactory httpClientFactory)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseLiveReload();
            }
            else
            {
                app.UseExceptionHandler("/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            //app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
                endpoints.MapRazorPages();
            });

            var server = Configuration["ServerName"];
            var httpClient = httpClientFactory.CreateClient();
            var addresses = app.ServerFeatures.Get<IServerAddressesFeature>().Addresses;
            var clientBaseAddress = addresses.First();

            JoinServer(httpClientFactory.CreateClient(),
                Configuration["GameServer"],
                Configuration["ClientCallbackAddress"],
                Configuration["PlayerName"]
                );
        }

        private async void JoinServer(HttpClient httpClient, string serverName, string clientBaseAddress, string userName)
        {
            await Task.Delay(TimeSpan.FromSeconds(5));
            var joinRequest = new JoinRequest { CallbackBaseAddress = clientBaseAddress, Name = userName };
            var joinResponse =  await httpClient.PostAsJsonAsync($"{serverName}/join", joinRequest);
        }
    }
}
