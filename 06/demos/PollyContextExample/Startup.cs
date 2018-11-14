using System;
using System.Diagnostics;
using System.Net.Http;
using System.Net.Http.Headers;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Polly;
using Polly.Retry;

namespace PollyContextExample
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

            RetryPolicy<HttpResponseMessage> httpRetryPolicy =
                Policy.HandleResult<HttpResponseMessage>(r => !r.IsSuccessStatusCode)
                    .RetryAsync(3, onRetry: (httpResponseMessage, retryCount, context) =>
                    {
                        if (context.ContainsKey("Host"))
                        {
                            Log($"Host:{context["Host"]}");
                        }
                        if (context.ContainsKey("CatalogId"))
                        {
                            Log($"CatalogId:{context["CatalogId"]}");
                        }
                        if (context.ContainsKey("User-Agent"))
                        {
                            Log($"User-Agent:{context["User-Agent"]}");
                        }

                        // you can add as my values to the context as you need and perfom other logic
                        //if (context.ContainsKey("SomeOtherValue"))
                        //{
                        //  perform some logic
                        //}
                    });

            services.AddSingleton<HttpClient>(httpClient);
            services.AddSingleton<RetryPolicy<HttpResponseMessage>>(httpRetryPolicy);
            services.AddMvc();
        }

        private void Log(string value)
        {
            Debug.WriteLine(value);
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
