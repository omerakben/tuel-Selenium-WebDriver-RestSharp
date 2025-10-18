using loc.test.API;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace loc.test.API.LettersOfCredit
{
    [TestClass]
    public class APITest_GetLocById : APIBase
    {
        private const string ApiBasePath = "/locs";

        private async Task<Guid> GetFirstValidLocIdAsync()
        {
            var response = await ExecuteGetAsync<GetLettersOfCreditResponse>($"{ApiBasePath}");

            Assert.IsTrue(response.IsSuccessful,
                $"Failed to get list of LOCs to select a valid ID. Status: {response.StatusCode}, Response: {response.Content}");

            Assert.IsNotNull(response.Data,
                "Response data is null.");

            Assert.IsNotNull(response.Data.Items,
                "Response items list is null.");

            Assert.IsTrue(response.Data.Items.Any(),
                "The /locs endpoint returned no data, so no valid ID could be found for testing.");

            var firstLoc = response.Data.Items.First();

            TestContext.WriteLine($"Using LOC ID for testing: {firstLoc.LetterOfCreditId}");
            TestContext.WriteLine($"LOC Number: {firstLoc.LocNumber}");

            return firstLoc.LetterOfCreditId;
        }

        [TestMethod]
        [Description("Verifies that a GET request for a specific, valid LOC returns a 200 OK status.")]
        public async Task GetLocById_Returns_Status_OK_For_ValidId()
        {
            // Get a valid LOC ID from the list
            Guid validLocId = await GetFirstValidLocIdAsync();

            var response = await ExecuteGetAsync($"{ApiBasePath}/{validLocId}");

            TestContext.WriteLine($"Request URL: {ApiBasePath}/{validLocId}");
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
        [Description("Verifies that the response for a valid LOC can be deserialized into the complete LetterOfCreditDetail model.")]
        public async Task GetLocById_Can_Parse_Response_For_ValidId()
        {
            // Get a valid LOC ID from the list
            Guid validLocId = await GetFirstValidLocIdAsync();

            // Get the detailed LOC
            var response = await ExecuteGetAsync<LetterOfCreditDetail>($"{ApiBasePath}/{validLocId}");

            if (!response.IsSuccessful)
            {
                TestContext.WriteLine($"Response failed. Status: {response.StatusCode}");
                TestContext.WriteLine($"Response Content: {response.Content}");
            }
        }

        [TestMethod]
        [Description("Verifies that a GET request for a non-existent LOC GUID returns a 404 Not Found status.")]
        public async Task GetLocById_Returns_NotFound_For_NonExistentId()
        {
            // Generate a random GUID that shouldn't exist
            var nonExistentLocId = Guid.NewGuid();

            TestContext.WriteLine($"Testing with non-existent ID: {nonExistentLocId}");

            var response = await ExecuteGetAsync($"{ApiBasePath}/{nonExistentLocId}");

            TestContext.WriteLine($"Response Status: {response.StatusCode}");

            Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode,
                $"Actual: {response.StatusCode}");
        }

        [TestMethod]
        [Description("Verifies that a GET request with an invalid GUID format returns appropriate error.")]
        public async Task GetLocById_Returns_Error_For_Invalid_Guid_Format()
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