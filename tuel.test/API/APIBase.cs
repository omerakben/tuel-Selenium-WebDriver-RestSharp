using OpenQA.Selenium;
using TUEL.TestFramework.API.Auth;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using RestSharp;
using System;
using System.Net;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.Globalization;
using System.Reflection.Metadata;
using Newtonsoft.Json.Linq;
using Octokit;
using System.Text.RegularExpressions;

namespace TUEL.TestFramework.API
{
    [TestClass]
    public abstract class APIBase : TestBase
    {
        protected static RestClient ApiClient => InitializeTestAssembly.ApiClient ?? throw new InvalidOperationException("API Client not initialized.");

        private async Task<RestResponse> ExecuteRequestAsync(RestRequest request)
        {
            try
            {
                var token = await EntraAuthHelper.GetAccessTokenAsync();
                request.AddHeader("Authorization", $"Bearer {token}");

                Console.WriteLine($"Executing {request.Method} request to: {request.Resource}");
                return await ApiClient.ExecuteAsync(request);
            }
            catch (Exception ex)
            {
                Assert.Fail($"An exception occurred during the API call: {ex}");
                return null;
            }
        }

        private async Task<RestResponse<T>> ExecuteRequestAsync<T>(RestRequest request)
        {
            try
            {
                var token = await EntraAuthHelper.GetAccessTokenAsync();
                request.AddHeader("Authorization", $"Bearer {token}");

                Console.WriteLine($"Executing {request.Method} request to: {request.Resource}");
                return await ApiClient.ExecuteAsync<T>(request);
            }
            catch (Exception ex)
            {
                Assert.Fail($"An exception occurred during the API call: {ex}");
                return null;
            }
        }

        protected async Task<RestResponse> ExecuteGetAsync(string resource) =>
            await ExecuteRequestAsync(new RestRequest(resource, Method.Get));

        protected async Task<RestResponse<T>> ExecuteGetAsync<T>(string resource) =>
            await ExecuteRequestAsync<T>(new RestRequest(resource, Method.Get));

        protected async Task<RestResponse> ExecutePostAsync(string resource, object body)
        {
            var request = new RestRequest(resource, Method.Post);
            request.AddJsonBody(body);
            return await ExecuteRequestAsync(request);
        }

        protected async Task<RestResponse<T>> ExecutePostAsync<T>(string resource, object body)
        {
            var request = new RestRequest(resource, Method.Post);
            request.AddJsonBody(body);
            return await ExecuteRequestAsync<T>(request);
        }

        protected async Task<RestResponse> ExecuteGetAsyncWithInvalidToken(string resource)
        {
            var request = new RestRequest(resource, Method.Get);
            request.AddHeader("Authorization", "Bearer INVALID_TEST_TOKEN");
            return await ApiClient.ExecuteAsync(request);
        }
    }
}
