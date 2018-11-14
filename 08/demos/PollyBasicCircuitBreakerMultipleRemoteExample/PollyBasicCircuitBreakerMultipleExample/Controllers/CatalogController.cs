﻿using System;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Polly;
using Polly.CircuitBreaker;
using Polly.Retry;

namespace PollyBasicCircuitBreakerMultipleExample.Controllers
{
    [Produces("application/json")]
    [Route("api/[controller]")]
    public class CatalogController : Controller
    {
        private readonly HttpClient _httpClient;
        readonly RetryPolicy<HttpResponseMessage> _httpRetryPolicy;

        private readonly CircuitBreakerPolicy<HttpResponseMessage> _breakerPolicy;
        public CatalogController(HttpClient httpClient, CircuitBreakerPolicy<HttpResponseMessage> breakerPolicy)
        {
            _breakerPolicy = breakerPolicy;
            _httpClient = httpClient;
            _httpRetryPolicy = Policy.HandleResult<HttpResponseMessage>(r => !r.IsSuccessStatusCode).RetryAsync(2);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> Get(int id)
        {
            string requestEndpoint = $"inventory/{id}";

            HttpResponseMessage response = await _httpRetryPolicy.ExecuteAsync(
                 () => _breakerPolicy.ExecuteAsync(
                     () => _httpClient.GetAsync(requestEndpoint)));

            if (response.IsSuccessStatusCode)
            {
                int itemsInStock = JsonConvert.DeserializeObject<int>(await response.Content.ReadAsStringAsync());
                return Ok(itemsInStock);
            }

            return StatusCode((int)response.StatusCode, response.Content.ReadAsStringAsync());
        }

        [HttpGet("pricing/{id}")]
        public async Task<IActionResult> GetPricing(int id)
        {
            string requestEndpoint = $"pricing/{id}";

            HttpResponseMessage response = await _httpRetryPolicy.ExecuteAsync(
                () => _breakerPolicy.ExecuteAsync(
                    () => _httpClient.GetAsync(requestEndpoint)));

            if (response.IsSuccessStatusCode)
            {
                decimal priceOfItem = JsonConvert.DeserializeObject<decimal>(await response.Content.ReadAsStringAsync());
                return Ok($"${priceOfItem}");
            }

            return StatusCode((int)response.StatusCode, response.Content.ReadAsStringAsync());
        }

    }
}
