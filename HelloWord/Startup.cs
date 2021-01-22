using Consul;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HelloWord
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
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });

            string serverName = Configuration.GetSection("Consul:ServiceName").Value;
            string clientUrl = Configuration.GetSection("Consul:ConsulAddress").Value;
            string agentIP = Configuration.GetSection("Consul:ServiceIP").Value;
            string port = Configuration.GetSection("Consul:ServicePort").Value;
            string heartbeatUrl = Configuration.GetSection("Consul:ServiceHealthCheck").Value;
            string tags = Configuration.GetSection("Consul:Tags").Value;
            var agentTags = new string[] { };
            if (!string.IsNullOrEmpty(tags))
            {
                agentTags = tags.Split(',');
            }

            ConsulClient client = new ConsulClient(obj =>
            {
                obj.Address = new Uri(clientUrl);
                obj.Datacenter = serverName;
            });

            client.Agent.ServiceRegister(new AgentServiceRegistration()
            {
                ID = $"{serverName}_{Guid.NewGuid()}",
                Name = serverName,
                Address = agentIP,
                Port = Convert.ToInt32(port),
                Tags = agentTags,
                Check = new AgentServiceCheck()
                {
                    DeregisterCriticalServiceAfter = TimeSpan.FromSeconds(5),
                    Interval = TimeSpan.FromSeconds(15),
                    HTTP = heartbeatUrl,
                    Timeout = TimeSpan.FromSeconds(5)
                }
            }).Wait();
        }
    }
}
