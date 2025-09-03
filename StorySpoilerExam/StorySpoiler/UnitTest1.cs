using System;
using NUnit.Framework;
using RestSharp;
using RestSharp.Authenticators;
using System.Net;
using System.Text.Json;
using StorySpoiler.Models;
namespace StorySpoiler
{
    [TestFixture]
    public class StorySpoilerTests
    {
        private RestClient client;
        private static string? lastCreatedStoryId;
        private static string baseUrl = "https://d3s5nxhwblsjbi.cloudfront.net";

        [OneTimeSetUp]
        public void Setup()
        {
            string token = GetJwtToken("qnko", "qnko1qnko");

            var options = new RestClientOptions(baseUrl)
            {
                Authenticator = new JwtAuthenticator(token)
            };

            client = new RestClient(options);
        }

        private string GetJwtToken(string username, string password)
        {
            var loginClient = new RestClient(baseUrl);

            var request = new RestRequest("/api/User/Authentication", Method.Post);
            request.AddJsonBody(new { username, password });

            var response = loginClient.Execute(request);

            var json = JsonSerializer.Deserialize<JsonElement>(response.Content);
            return json.GetProperty("accessToken").GetString();
        }

        [Test, Order(1)]
        public void CreateNewStorySpoiler_ShouldReturnSuccess()
        {
            var story = new StoryDTO
            {
                Title = "New Story",
                Description = "Test story description",
                Url = ""
            };

            var request = new RestRequest("/api/Story/Create", Method.Post);
            request.AddJsonBody(story);

            var response = client.Execute(request);

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Created));

            var json = JsonSerializer.Deserialize<JsonElement>(response.Content);

            lastCreatedStoryId = json.GetProperty("storyId").GetString();

            Assert.That(lastCreatedStoryId, Is.Not.Null.Or.Empty);
            Assert.That(response.Content, Does.Contain("Successfully created!"));
        }

        [Test, Order(2)]
        public void EditLastCreatedStory_ShouldReturnSuccess()
        {
            var edit = new StoryDTO
            {
                Title = "First created story is about to be edited",
                Description = "This story was edited successfully",
                Url = ""
            };
            var request = new RestRequest($"/api/Story/Edit/{lastCreatedStoryId}", Method.Put);
            request.AddJsonBody(edit);

            var response = client.Execute(request);

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));

            var json = JsonSerializer.Deserialize<JsonElement>(response.Content);
            Assert.That(response.Content, Does.Contain("Successfully edited"));
        }

        [Test, Order(3)]
        public void GetAllStorySpoilers_ShouldReturnSuccess()
        {
            var request = new RestRequest("/api/Story/All", Method.Get);
            var response = client.Execute(request);

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            Assert.That(response.Content, Is.Not.Empty);
        }

        [Test, Order(4)]
        public void DeleteStorySpoiler_ShouldReturnSuccess()
        {
            var request = new RestRequest($"/api/Story/Delete/{lastCreatedStoryId}", Method.Delete);
            var response = client.Execute(request);

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            Assert.That(response.Content, Does.Contain("Deleted successfully!"));
        }

        [Test, Order(5)]
        public void CreateStoryWithoutRequiredFields_ShouldReturnError()
        {
            var missingFields = new StoryDTO
            {
                Title = "Error",
                Url = ""
            };
            var request = new RestRequest("/api/Story/Create", Method.Post);
            request.AddJsonBody(missingFields);
            var response = client.Execute(request);

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
        }

        [Test, Order(6)]
        public void EditNonExistingStory_ShouldReturnError()
        {
            string nonExistingStoryId = "7260140jjpgtyqqa123";
            var parameters = new StoryDTO
            {
                Title = "Error",
                Description = "Error",
                Url = ""
            };
            var request = new RestRequest($"/api/Story/Edit/{nonExistingStoryId}", Method.Put);
            request.AddJsonBody(parameters);
            var response = client.Execute(request);

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.NotFound));
            Assert.That(response.Content, Does.Contain("No spoilers..."));
        }

        [Test, Order(7)]
        public void DeleteNonExistingStory_ShouldReturnError()
        {
            string randomStoryId = "jqwe129u41haas901nihs123";
            var request = new RestRequest($"/api/Story/Delete/{randomStoryId}", Method.Delete);
            var response = client.Execute(request);

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
            Assert.That(response.Content, Does.Contain("Unable to delete this story spoiler!"));
        }

            [OneTimeTearDown]
        public void Cleanup()
        {
            client?.Dispose();
        }

    }
}