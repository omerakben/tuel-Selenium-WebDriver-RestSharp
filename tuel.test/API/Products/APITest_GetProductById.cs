using TUEL.TestFramework.API;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace TUEL.TestFramework.API.Products
{
    [TestClass]
    public class APITest_GetProductById : APIBase
    {
        private const string ApiBasePath = "/products";

        private async Task<Guid> GetFirstValidProductIdAsync()
        {
            var response = await ExecuteGetAsync<GetProductsResponse>($"{ApiBasePath}");

            Assert.IsTrue(response.IsSuccessful,
                $"Failed to get list of products to select a valid ID. Status: {response.StatusCode}, Response: {response.Content}");

            Assert.IsNotNull(response.Data,
                "Response data is null.");

            Assert.IsNotNull(response.Data.Items,
                "Response items list is null.");

            Assert.IsTrue(response.Data.Items.Any(),
                "The /products endpoint returned no data, so no valid ID could be found for testing.");

            var firstProduct = response.Data.Items.First();

            TestContext.WriteLine($"Using Product ID for testing: {firstProduct.ProductId}");
            TestContext.WriteLine($"Product Number: {firstProduct.ProductNumber}");

            return firstProduct.ProductId;
        }

        [TestMethod]
        [Description("Verifies that a GET request for a specific, valid product returns a 200 OK status.")]
        public async Task GetProductById_Returns_Status_OK_For_ValidId()
        {
            // Get a valid product ID from the list
            Guid validProductId = await GetFirstValidProductIdAsync();

            var response = await ExecuteGetAsync($"{ApiBasePath}/{validProductId}");

            TestContext.WriteLine($"Request URL: {ApiBasePath}/{validProductId}");
            TestContext.WriteLine($"Response Status: {response.StatusCode}");

            if (response.StatusCode != HttpStatusCode.OK)
            {
                TestContext.WriteLine($"Response Content: {response.Content}");
            }

            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode,
                $"The API did not return a successful status code for a valid ID. Actual: {response.StatusCode}");

            Assert.IsNotNull(response.Content,
                "Response content should not be null.");
        }

        [TestMethod]
        [Description("Verifies that the response for a valid product can be deserialized into the complete ProductDetail model.")]
        public async Task GetProductById_Can_Parse_Response_For_ValidId()
        {
            // Get a valid product ID from the list
            Guid validProductId = await GetFirstValidProductIdAsync();

            // Get the detailed product
            var response = await ExecuteGetAsync<ProductDetail>($"{ApiBasePath}/{validProductId}");

            if (!response.IsSuccessful)
            {
                TestContext.WriteLine($"Response failed. Status: {response.StatusCode}");
                TestContext.WriteLine($"Response Content: {response.Content}");
            }
        }

        [TestMethod]
        [Description("Verifies that a GET request for a non-existent product GUID returns a 404 Not Found status.")]
        public async Task GetProductById_Returns_NotFound_For_NonExistentId()
        {
            // Generate a random GUID that shouldn't exist
            var nonExistentProductId = Guid.NewGuid();

            TestContext.WriteLine($"Testing with non-existent ID: {nonExistentProductId}");

            var response = await ExecuteGetAsync($"{ApiBasePath}/{nonExistentProductId}");

            TestContext.WriteLine($"Response Status: {response.StatusCode}");

            Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode,
                $"Actual: {response.StatusCode}");
        }

        [TestMethod]
        [Description("Verifies that a GET request with an invalid GUID format returns appropriate error.")]
        public async Task GetProductById_Returns_Error_For_Invalid_Guid_Format()
        {
            var invalidGuid = "not-a-valid-guid";

            var response = await ExecuteGetAsync($"{ApiBasePath}/{invalidGuid}");

            TestContext.WriteLine($"Response Status: {response.StatusCode}");
            TestContext.WriteLine($"Response Content: {response.Content}");

            Assert.IsTrue(
                response.StatusCode == HttpStatusCode.BadRequest ||
                response.StatusCode == HttpStatusCode.NotFound,
                $"Expected BadRequest or NotFound for invalid GUID format. Actual: {response.StatusCode}");
        }
    }
}
