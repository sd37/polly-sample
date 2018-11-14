using System;
using System.Diagnostics;
using System.Net.Http;
using System.Net.Http.Headers;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Polly;
using Polly.CircuitBreaker;

namespace PollyBasicCircuitBreakerMultipleExample
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
            CircuitBreakerPolicy<HttpResponseMessage> breakerPolicy = Policy
                .HandleResult<HttpResponseMessage>(r => !r.IsSuccessStatusCode)
                .CircuitBreakerAsync(2, TimeSpan.FromSeconds(60), OnBreak, OnReset, OnHalfOpen);

            HttpClient httpClient = new HttpClient()
            {
                BaseAddress = new Uri("http://localhost:57696/api/") // this is the endpoint HttpClient will hit,
            };
            httpClient.DefaultRequestHeaders.Accept.Clear();
            httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            services.AddSingleton<HttpClient>(httpClient);
            services.AddSingleton<CircuitBreakerPolicy<HttpResponseMessage>>(breakerPolicy);
            services.AddMvc();
        }









        private void OnHalfOpen()
        {
            Debug.WriteLine("Connection half open");
        }

        private void OnReset(Context context)
        {
            Debug.WriteLine("Connection reset");
        }

        private void OnBreak(DelegateResult<HttpResponseMessage> delegateResult, TimeSpan timeSpan, Context context)
        {
            Debug.WriteLine($"Connection break: {delegateResult.Result}, {delegateResult.Result}");
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
