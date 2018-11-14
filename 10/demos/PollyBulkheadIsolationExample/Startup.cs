using System;
using System.Diagnostics;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Polly;
using Polly.Bulkhead;
using Polly.CircuitBreaker;

namespace PollyBulkheadIsolationExample
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
            BulkheadPolicy<HttpResponseMessage> bulkheadIsolationPolicy = Policy
                .BulkheadAsync<HttpResponseMessage>(2, 4, onBulkheadRejectedAsync: OnBulkheadRejectedAsync);

            HttpClient httpClient = new HttpClient()
            {
                BaseAddress = new Uri("http://localhost:57696/api/") // this is the endpoint HttpClient will hit,
            };
            httpClient.DefaultRequestHeaders.Accept.Clear();
            httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            services.AddSingleton<HttpClient>(httpClient);
            services.AddSingleton<BulkheadPolicy<HttpResponseMessage>>(bulkheadIsolationPolicy);
            services.AddMvc();
        }

        private Task OnBulkheadRejectedAsync(Context context)
        {
            Debug.WriteLine($"PollyDemo OnBulkheadRejectedAsync Executed");
            return Task.CompletedTask;
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
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
