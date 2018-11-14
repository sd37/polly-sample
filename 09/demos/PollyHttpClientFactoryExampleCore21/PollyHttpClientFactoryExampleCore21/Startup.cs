using System;
using System.Net.Http;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Polly;
using Polly.Registry;

namespace PollyHttpClientFactoryExampleCore21
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
            IPolicyRegistry<string> registry = services.AddPolicyRegistry();

            IAsyncPolicy<HttpResponseMessage> httpRetryPolicy =
                Policy.HandleResult<HttpResponseMessage>(r => !r.IsSuccessStatusCode)
                    .RetryAsync(3);

            registry.Add("SimpleHttpRetryPolicy", httpRetryPolicy);

            IAsyncPolicy<HttpResponseMessage> httWaitAndpRetryPolicy =
                Policy.HandleResult<HttpResponseMessage>(r => !r.IsSuccessStatusCode)
                    .WaitAndRetryAsync(3, retryAttempt => TimeSpan.FromSeconds(retryAttempt));

            registry.Add("SimpleWaitAndRetryPolicy", httWaitAndpRetryPolicy);

            IAsyncPolicy<HttpResponseMessage> noOpPolicy = Policy.NoOpAsync()
                .AsAsyncPolicy<HttpResponseMessage>();

            registry.Add("NoOpPolicy", noOpPolicy);

            services.AddHttpClient("RemoteServer", client =>
            {
                client.BaseAddress = new Uri("http://localhost:57696/api/");
                client.DefaultRequestHeaders.Add("Accept", "application/json");
            }).AddPolicyHandlerFromRegistry((PolicySelector));

            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_1);
        }

        private IAsyncPolicy<HttpResponseMessage> PolicySelector(IReadOnlyPolicyRegistry<string> policyRegistry, 
            HttpRequestMessage httpRequestMessage)
        {
            if (httpRequestMessage.RequestUri.LocalPath.StartsWith("find"))
            {
                return policyRegistry.Get<IAsyncPolicy<HttpResponseMessage>>("SimpleHttpRetryPolicy");
            }
            else if (httpRequestMessage.RequestUri.LocalPath.StartsWith("create"))
            {
                return policyRegistry.Get<IAsyncPolicy<HttpResponseMessage>>("NoOpPolicy");
            }
            else
            {
                return policyRegistry.Get<IAsyncPolicy<HttpResponseMessage>>("SimpleWaitAndRetryPolicy");
            }
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

//public void ConfigureServices(IServiceCollection services)
//{
//    IPolicyRegistry<string> registry = services.AddPolicyRegistry();

//    IAsyncPolicy<HttpResponseMessage> httpRetryPolicy =
//        Policy.HandleResult<HttpResponseMessage>(r => !r.IsSuccessStatusCode)
//            .RetryAsync(3);

//    registry.Add("SimpleHttpRetryPolicy", httpRetryPolicy);

//    IAsyncPolicy<HttpResponseMessage> httWaitAndpRetryPolicy =
//        Policy.HandleResult<HttpResponseMessage>(r => !r.IsSuccessStatusCode)
//            .WaitAndRetryAsync(3, retryAttempt => TimeSpan.FromSeconds(retryAttempt));

//    registry.Add("SimpleWaitAndRetryPolicy", httWaitAndpRetryPolicy);

//    IAsyncPolicy<HttpResponseMessage> noOpPolicy = Policy.NoOpAsync()
//        .AsAsyncPolicy<HttpResponseMessage>();

//    registry.Add("NoOpPolicy", noOpPolicy);

//    services.AddHttpClient("RemoteServer", client =>
//    {
//        client.BaseAddress = new Uri("http://localhost:57696/api/");
//        client.DefaultRequestHeaders.Add("Accept", "application/json");
//    }).AddPolicyHandlerFromRegistry((policyRegistry, httpRequestMessage) =>
//    {
//        if (httpRequestMessage.RequestUri.LocalPath.StartsWith("find"))
//        {
//            return policyRegistry.Get<IAsyncPolicy<HttpResponseMessage>>("SimpleHttpRetryPolicy");
//        }
//        else if (httpRequestMessage.RequestUri.LocalPath.StartsWith("create"))
//        {
//            return policyRegistry.Get<IAsyncPolicy<HttpResponseMessage>>("NoOpPolicy");
//        }
//        else
//        {
//            return policyRegistry.Get<IAsyncPolicy<HttpResponseMessage>>("SimpleWaitAndRetryPolicy");
//        }
//    });
//    //}).AddPolicyHandlerFromRegistry((policyRegistry, httpRequestMessage) =>
//    //{
//    //    if (httpRequestMessage.Method == HttpMethod.Get)
//    //    {
//    //        return policyRegistry.Get<IAsyncPolicy<HttpResponseMessage>>("SimpleHttpRetryPolicy");
//    //    }
//    //    else if (httpRequestMessage.Method == HttpMethod.Post)
//    //    {
//    //        return policyRegistry.Get<IAsyncPolicy<HttpResponseMessage>>("NoOpPolicy");
//    //    }
//    //    else
//    //    {
//    //        return policyRegistry.Get<IAsyncPolicy<HttpResponseMessage>>("SimpleWaitAndRetryPolicy");
//    //    }
//    //});

//    services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_1);
//}

