using TUEL.TestFramework.API;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace TUEL.TestFramework.API.Products
{
    #region Response Models

    public class ProductListItem
    {
        [JsonProperty("productId")]
        public Guid ProductId { get; set; }

        [JsonProperty("productDataId")]
        public int? ProductDataId { get; set; }

        [JsonProperty("productNumber")]
        public string ProductNumber { get; set; }

        [JsonProperty("productType")]
        public string ProductType { get; set; }

        [JsonProperty("status")]
        public string Status { get; set; }

        [JsonProperty("price")]
        public decimal? Price { get; set; }

        [JsonProperty("createdDate")]
        public DateTimeOffset? CreatedDate { get; set; }

        [JsonProperty("submittedDate")]
        public DateTimeOffset? SubmittedDate { get; set; }

        [JsonProperty("expirationDate")]
        public DateTimeOffset? ExpirationDate { get; set; }

        [JsonProperty("customerName")]
        public string CustomerName { get; set; }
    }

    public class GetProductsResponse
    {
        [JsonProperty("items")]
        public List<ProductListItem> Items { get; set; }

        [JsonProperty("pageOffset")]
        public int PageOffset { get; set; }

        [JsonProperty("pageSize")]
        public int PageSize { get; set; }

        [JsonProperty("sortColumns")]
        public string SortColumns { get; set; }

        [JsonProperty("sortDirection")]
        public string SortDirection { get; set; }

        [JsonProperty("totalItems")]
        public int TotalItems { get; set; }

        [JsonProperty("totalPages")]
        public int TotalPages { get; set; }
    }

    public class Customer
    {
        [JsonProperty("customerId")]
        public int CustomerId { get; set; }

        [JsonProperty("customerName")]
        public string CustomerName { get; set; }

        [JsonProperty("street")]
        public string Street { get; set; }

        [JsonProperty("city")]
        public string City { get; set; }

        [JsonProperty("state")]
        public string State { get; set; }

        [JsonProperty("zip")]
        public string Zip { get; set; }
    }

    public class StatusHistoryItem
    {
        [JsonProperty("productId")]
        public Guid ProductId { get; set; }

        [JsonProperty("productDataStatusChangeId")]
        public int ProductDataStatusChangeId { get; set; }

        [JsonProperty("productDataStatusId")]
        public int ProductDataStatusId { get; set; }

        [JsonProperty("status")]
        public string Status { get; set; }

        [JsonProperty("statusDate")]
        public DateTime StatusDate { get; set; }

        [JsonProperty("enteredBy")]
        public string EnteredBy { get; set; }
    }

    public class StatusHistory
    {
        [JsonProperty("items")]
        public List<StatusHistoryItem> Items { get; set; }
    }

    public class ProductDetail
    {
        [JsonProperty("productId")]
        public Guid ProductId { get; set; }

        [JsonProperty("productDataId")]
        public int ProductDataId { get; set; }

        [JsonProperty("productType")]
        public string ProductType { get; set; }

        [JsonProperty("createdDate")]
        public DateTimeOffset CreatedDate { get; set; }

        [JsonProperty("submittedDate")]
        public DateTimeOffset? SubmittedDate { get; set; }

        [JsonProperty("modifiedDate")]
        public DateTimeOffset? ModifiedDate { get; set; }

        [JsonProperty("expirationDate")]
        public DateTimeOffset ExpirationDate { get; set; }

        [JsonProperty("issuedDate")]
        public DateTimeOffset? IssuedDate { get; set; }

        [JsonProperty("createdBy")]
        public string CreatedBy { get; set; }

        [JsonProperty("modifiedBy")]
        public string ModifiedBy { get; set; }

        [JsonProperty("submittedBy")]
        public string SubmittedBy { get; set; }

        [JsonProperty("productNumber")]
        public string ProductNumber { get; set; }

        [JsonProperty("price")]
        public decimal Price { get; set; }

        [JsonProperty("accountNumber")]
        public string AccountNumber { get; set; }

        [JsonProperty("organization")]
        public string Organization { get; set; }

        [JsonProperty("customer")]
        public Customer Customer { get; set; }

        [JsonProperty("statusHistory")]
        public StatusHistory StatusHistory { get; set; }

        [JsonProperty("status")]
        public string Status { get; set; }
    }

    #endregion

    [TestClass]
    public class APITest_GetProducts : APIBase
    {
        private const string ApiPath = "/products";

        [TestMethod]
        [Description("Verifies that a GET request to the products endpoint returns a successful status code.")]
        public async Task GetProducts_Returns_Status_OK()
        {
            var response = await ExecuteGetAsync(ApiPath);

            // Log the raw response for debugging
            TestContext.WriteLine($"Response Status: {response.StatusCode}");
            TestContext.WriteLine($"Response Content: {response.Content}");

            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode,
                $"The API did not return a successful status code. Actual: {response.StatusCode}, Content: {response.Content}");
        }

        [TestMethod]
        [Description("Verifies that the response from the products endpoint can be successfully deserialized.")]
        public async Task GetProducts_Can_Parse_Response()
        {
            var response = await ExecuteGetAsync<GetProductsResponse>(ApiPath);

            // Log for debugging
            if (!response.IsSuccessful)
            {
                TestContext.WriteLine($"Response failed. Status: {response.StatusCode}, Content: {response.Content}");
            }

            Assert.IsTrue(response.IsSuccessful,
                $"The API call was not successful. Status: {response.StatusCode}, Response: {response.Content}");

            Assert.IsNotNull(response.Data,
                "The deserialized response data was null. This indicates a mismatch between the model and JSON response.");

            Assert.IsNotNull(response.Data.Items,
                "The 'items' list within the response data was null.");

            Assert.IsTrue(response.Data.Items.Any(),
                "The 'items' list should not be empty.");

            Assert.IsTrue(response.Data.TotalItems > 0,
                $"TotalItems should be greater than 0. Actual: {response.Data.TotalItems}");

            Assert.IsTrue(response.Data.PageSize > 0,
                $"PageSize should be greater than 0. Actual: {response.Data.PageSize}");

            // Log first item for debugging
            var firstItem = response.Data.Items.FirstOrDefault();
            if (firstItem != null)
            {
                TestContext.WriteLine($"First Product ID: {firstItem.ProductId}");
                TestContext.WriteLine($"First Product Number: {firstItem.ProductNumber}");
                TestContext.WriteLine($"First Product Status: {firstItem.Status}");
            }
        }

        [TestMethod]
        [Description("Verifies that the items in the response have valid data.")]
        public async Task GetProducts_Items_Have_Valid_Data()
        {
            var response = await ExecuteGetAsync<GetProductsResponse>(ApiPath);

            Assert.IsTrue(response.IsSuccessful, "The API call was not successful.");
            Assert.IsNotNull(response.Data?.Items, "Response items are null.");
            Assert.IsTrue(response.Data.Items.Any(), "No items returned.");

            var firstItem = response.Data.Items.First();

            // Validate required fields
            Assert.AreNotEqual(Guid.Empty, firstItem.ProductId,
                "Product ID should not be empty GUID.");

            Assert.IsFalse(string.IsNullOrWhiteSpace(firstItem.ProductNumber),
                "Product Number should not be null or empty.");
        }
    }
}
