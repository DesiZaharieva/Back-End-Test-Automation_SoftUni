using RestSharp;
using RestSharp.Authenticators;
using RevueTestExam.Models;
using System.Net;
using System.Text.Json;

namespace RevueTestExam
{
    public class RevueAPITests
    {
        private RestClient client;
        private const string BASEURL = "https://d2925tksfvgq8c.cloudfront.net/";
        private const string EMAIL = "kuku@test.bg";
        private const string PASSWORD = "12345!";
        
        private static string lastRevueId;

        [OneTimeSetUp]
        public void Setup()
        {
            string jwtToken = GetJwtToken(EMAIL, PASSWORD);

            var options = new RestClientOptions(BASEURL)
            {
                Authenticator = new JwtAuthenticator(jwtToken)
            };

            client = new RestClient(options);
        }
        private string GetJwtToken(string email, string password)
        {
            RestClient authClient = new RestClient(BASEURL);
            var request = new RestRequest("/api/User/Authentication");
            request.AddJsonBody(new
            {
                email,
                password

            });
            var response = authClient.Execute(request, Method.Post);

            if (response.StatusCode == HttpStatusCode.OK)
            {
                JsonElement content = JsonSerializer.Deserialize<JsonElement>(response.Content);
                var token = content.GetProperty("accessToken").GetString();
                if (string.IsNullOrWhiteSpace(token))
                {
                    throw new InvalidOperationException("Access Token is null or empty");
                }

                return token;
            }
            else
            {
                throw new InvalidOperationException($"Unexpected response type {response.StatusCode} with data {response.Content}");
            }
        }

        [Test, Order(1)]
        public void CreateANewRevueWithTheRequiredFields_ShouldSucceed()
        {
            var requestData = new RevueDTO()
            {
                Title = "TestTitle",
                Description = "TestDescription",
            };

            var request = new RestRequest("/api/Revue/Create");
            request.AddJsonBody(requestData);

            var response = client.Execute(request, Method.Post);
            var responseData = JsonSerializer.Deserialize<ApiResponseDTO>(response.Content);

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            Assert.That(responseData.Msg, Is.EqualTo("Successfully created!"));
        }

        [Test, Order(2)]
        public void GetAllRevues_ShouldReturnNonEmptyArray()
        {


            var request = new RestRequest("/api/Revue/All");


            var response = client.Execute(request, Method.Get);
            var responseDataArray = JsonSerializer.Deserialize<ApiResponseDTO[]>(response.Content);

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            Assert.That(responseDataArray.Length, Is.GreaterThan(0));

            var lastRevueId = responseDataArray[responseDataArray.Length - 1].RevueId;
        }

        [Test, Order(3)]
        public void EditRevie_WithCorrectData_ShouldSucceed()
        {
            var requestData = new RevueDTO()
            {
                Title = "editedTitle",
                Description = "TestDescription",
            };

            var request = new RestRequest("/api/Revue/Edit");
            request.AddQueryParameter("revueId", lastRevueId);
            request.AddJsonBody(requestData);

            var response = client.Execute(request, Method.Put);
            var responseData = JsonSerializer.Deserialize<ApiResponseDTO>(response.Content);

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            Assert.That(responseData.Msg, Is.EqualTo("Successfully edited"));
        }

        [Test, Order(4)]
        public void DeleteRevue_ShouldSucceed()
        {

            var request = new RestRequest("/api/Revue/Delete");
            request.AddQueryParameter("revueId", lastRevueId);


            var response = client.Execute(request, Method.Delete);


            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            Assert.That(response.Content, Does.Contain("The revue is deleted!"));
        }

        [Test, Order(5)]
        public void CreateNewRevue_WithoutCorrectData_ShouldFail()
        {
            var requestData = new RevueDTO()
            {
                Title = "TestTitle",

            };

            var request = new RestRequest("/api/Revue/Create");
            request.AddJsonBody(requestData);

            var response = client.Execute(request, Method.Post);
            var responseData = JsonSerializer.Deserialize<ApiResponseDTO>(response.Content);

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));

        }
        [Test, Order(6)]
        public void EditNonExistingRevue_ShouldFail()
        {

            var editRequest = new RevueDTO()
            {
                Title = "Edited R",
                Description = "Updated description."

            };

            var request = new RestRequest($"/api/Revue/Edit/{lastRevueId}" + 2);

            request.AddJsonBody(editRequest);
            var response = client.Execute(request, Method.Put);
            var editResponse = JsonSerializer.Deserialize<ApiResponseDTO>(response.Content);

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.NotFound));
            Assert.That(editResponse.Msg, Is.EqualTo("There is no such revue!"));
        }


        [Test, Order(7)]
        public void DeleteNonExistingStory_ShouldFail()
        {

            var request = new RestRequest("/api/Revue/Delete/21353242");
            var response = client.Execute(request, Method.Delete);
            var editResponse = JsonSerializer.Deserialize<ApiResponseDTO>(response.Content);

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
            Assert.That(editResponse.Msg, Is.EqualTo("There is no such revue!"));
        }
    }
}