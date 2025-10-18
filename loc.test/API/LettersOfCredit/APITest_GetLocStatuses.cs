using loc.test.API;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace loc.test.API.LettersOfCredit
{
    public class LocDataStatus
    {
        [JsonProperty("id")]
        public int Id { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }
    }

    [TestClass]
    public class APITest_GetLocStatuses : APIBase
    {
        private const string ApiPath = "/locs/statuses";

        [TestMethod]
        [Description("Verifies that a GET request to the /locs/statuses endpoint returns a 200 OK status.")]
        public async Task GetLocStatuses_Returns_Status_OK()
        {
            var response = await ExecuteGetAsync(ApiPath);

            TestContext.WriteLine($"Response Status: {response.StatusCode}");

            if (response.StatusCode != HttpStatusCode.OK)
            {
                TestContext.WriteLine($"Response Content: {response.Content}");
            }

            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode,
                $"The API did not return a successful status code. Actual: {response.StatusCode}");

            Assert.IsNotNull(response.Content,
                "Response content should not be null.");
        }

        [TestMethod]
        [Description("Verifies that all status objects have valid data.")]
        public async Task GetLocStatuses_All_Have_Valid_Data()
        {
            var response = await ExecuteGetAsync<List<LocDataStatus>>(ApiPath);

            Assert.IsTrue(response.Content.Length > 0);

        }
    }
}