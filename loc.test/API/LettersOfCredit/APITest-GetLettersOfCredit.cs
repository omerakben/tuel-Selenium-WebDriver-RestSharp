using loc.test.API;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace loc.test.API.LettersOfCredit
{
    #region Response Models

    public class LetterOfCreditListItem
    {
        [JsonProperty("letterOfCreditId")]
        public Guid LetterOfCreditId { get; set; }

        [JsonProperty("locDataId")]
        public int? LocDataId { get; set; }

        [JsonProperty("locNumber")]
        public string LocNumber { get; set; }

        [JsonProperty("locType")]
        public string LocType { get; set; }

        [JsonProperty("status")]
        public string Status { get; set; }

        [JsonProperty("amount")]
        public decimal? Amount { get; set; }

        [JsonProperty("createdDate")]
        public DateTimeOffset? CreatedDate { get; set; }

        [JsonProperty("submittedDate")]
        public DateTimeOffset? SubmittedDate { get; set; }

        [JsonProperty("expirationDate")]
        public DateTimeOffset? ExpirationDate { get; set; }

        [JsonProperty("beneficiaryName")]
        public string BeneficiaryName { get; set; }
    }

    public class GetLettersOfCreditResponse
    {
        [JsonProperty("items")]
        public List<LetterOfCreditListItem> Items { get; set; }

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

    public class Beneficiary
    {
        [JsonProperty("beneficiaryId")]
        public int BeneficiaryId { get; set; }

        [JsonProperty("beneficiaryName")]
        public string BeneficiaryName { get; set; }

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
        [JsonProperty("letterOfCreditId")]
        public Guid LetterOfCreditId { get; set; }

        [JsonProperty("locDataStatusChangeId")]
        public int LocDataStatusChangeId { get; set; }

        [JsonProperty("locDataStatusId")]
        public int LocDataStatusId { get; set; }

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

    public class LetterOfCreditDetail
    {
        [JsonProperty("letterOfCreditId")]
        public Guid LetterOfCreditId { get; set; }

        [JsonProperty("locDataId")]
        public int LocDataId { get; set; }

        [JsonProperty("locType")]
        public string LocType { get; set; }

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

        [JsonProperty("locNumber")]
        public string LocNumber { get; set; }

        [JsonProperty("amount")]
        public decimal Amount { get; set; }

        [JsonProperty("dda")]
        public string Dda { get; set; }

        [JsonProperty("memberInstitution")]
        public string MemberInstitution { get; set; }

        [JsonProperty("beneficiary")]
        public Beneficiary Beneficiary { get; set; }

        [JsonProperty("statusHistory")]
        public StatusHistory StatusHistory { get; set; }

        [JsonProperty("status")]
        public string Status { get; set; }
    }

    #endregion

    [TestClass]
    public class APITest_GetLettersOfCredit : APIBase
    {
        private const string ApiPath = "/locs";

        [TestMethod]
        [Description("Verifies that a GET request to the letters-of-credit endpoint returns a successful status code.")]
        public async Task GetLettersOfCredit_Returns_Status_OK()
        {
            var response = await ExecuteGetAsync(ApiPath);

            // Log the raw response for debugging
            TestContext.WriteLine($"Response Status: {response.StatusCode}");
            TestContext.WriteLine($"Response Content: {response.Content}");

            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode,
                $"The API did not return a successful status code. Actual: {response.StatusCode}, Content: {response.Content}");
        }

        [TestMethod]
        [Description("Verifies that the response from the letters-of-credit endpoint can be successfully deserialized.")]
        public async Task GetLettersOfCredit_Can_Parse_Response()
        {
            var response = await ExecuteGetAsync<GetLettersOfCreditResponse>(ApiPath);

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
                TestContext.WriteLine($"First LOC ID: {firstItem.LetterOfCreditId}");
                TestContext.WriteLine($"First LOC Number: {firstItem.LocNumber}");
                TestContext.WriteLine($"First LOC Status: {firstItem.Status}");
            }
        }

        [TestMethod]
        [Description("Verifies that the items in the response have valid data.")]
        public async Task GetLettersOfCredit_Items_Have_Valid_Data()
        {
            var response = await ExecuteGetAsync<GetLettersOfCreditResponse>(ApiPath);

            Assert.IsTrue(response.IsSuccessful, "The API call was not successful.");
            Assert.IsNotNull(response.Data?.Items, "Response items are null.");
            Assert.IsTrue(response.Data.Items.Any(), "No items returned.");

            var firstItem = response.Data.Items.First();

            // Validate required fields
            Assert.AreNotEqual(Guid.Empty, firstItem.LetterOfCreditId,
                "Letter of Credit ID should not be empty GUID.");

            Assert.IsFalse(string.IsNullOrWhiteSpace(firstItem.LocNumber),
                "LOC Number should not be null or empty.");
        }
    }
}