using System;
using System.Net.Http;
using System.Net.Http.Headers;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Polly;
using Polly.Registry;

namespace PollyRegistryExample
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
            HttpClient httpClient = new HttpClient()
            {
                BaseAddress = new Uri("http://localhost:57696/api/") // this is the endpoint HttpClient will hit,
            };

            httpClient.DefaultRequestHeaders.Accept.Clear();
            httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            services.AddSingleton<HttpClient>(httpClient);

            IAsyncPolicy<HttpResponseMessage> httpRetryPolicy =
                Policy.HandleResult<HttpResponseMessage>(r => !r.IsSuccessStatusCode)
                    .RetryAsync(3);

            services.AddSingleton<IAsyncPolicy<HttpResponseMessage>>(httpRetryPolicy);
            services.AddSingleton<IPolicyRegistry<string>>(GetRegistry());

            services.AddMvc();
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

        private IPolicyRegistry<string> GetRegistry()
        {
            IPolicyRegistry<string> registry = new PolicyRegistry();

            IAsyncPolicy<HttpResponseMessage> httpRetryPolicy =
                Policy.HandleResult<HttpResponseMessage>(r => !r.IsSuccessStatusCode)
                    .RetryAsync(3);

            registry.Add("SimpleHttpRetryPolicy", httpRetryPolicy);

            IAsyncPolicy httpClientTimeoutExceptionPolicy = Policy.Handle<HttpRequestException>()
                .WaitAndRetryAsync(1, retryAttempt => TimeSpan.FromSeconds(retryAttempt));

            registry.Add("SimpleHttpTimeoutPolicy", httpClientTimeoutExceptionPolicy);

            return registry;
        }
    }
}
