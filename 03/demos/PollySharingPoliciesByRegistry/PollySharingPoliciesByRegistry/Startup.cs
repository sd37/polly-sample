using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Polly.Registry;
using Polly.Retry;
using System.Net.Http;
using Polly;

namespace PollySharingPoliciesByRegistry
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
            services.AddSingleton<PolicyRegistry>(GetRegistry());
            services.AddMvc();
        }

        private PolicyRegistry GetRegistry()
        {
            PolicyRegistry registry = new PolicyRegistry();

            IAsyncPolicy<HttpResponseMessage> httpRetryPolicy =
               Policy.HandleResult<HttpResponseMessage>(r => !r.IsSuccessStatusCode)
               .WaitAndRetryAsync(3, retryAttempt => TimeSpan.FromSeconds(retryAttempt));

            registry.Add("SimpleHttpWaitAndRetry", httpRetryPolicy);

            IAsyncPolicy httpClientTimeoutException = Policy.Handle<HttpRequestException>()
               .WaitAndRetryAsync(1, retryAttempt => TimeSpan.FromSeconds(retryAttempt));

            registry.Add("HttpClientTimeout", httpClientTimeoutException);

            return registry;
        }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseMvc();
        }

       
    }
}
