namespace Natech.Caas.API.IntegrationTests;

using System;
using System.Net;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Bogus;
using Microsoft.Extensions.DependencyInjection;
using Natech.Caas.API.Database;
using Natech.Caas.API.Database.Entities;
using Natech.Caas.API.Dtos;

public class CatsControllerTests : BaseTest
{

    #region List Cats Tests
    [Test]
    public async Task ListCats_ShouldReturnEmptyList_WhenNoCatsExist()
    {
        // Act
        var response = await _client.GetAsync("/api/cats?page=1&pageSize=2");

        // Assert
        response.EnsureSuccessStatusCode();
        var responsePayload = await response.Content.ReadAsStringAsync();
        var catResponse = JsonSerializer.Deserialize<ListResponse<CatDto>>(responsePayload, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        });

        Assert.IsNotNull(catResponse);
        Assert.IsEmpty(catResponse.Data);
        Assert.That(catResponse.TotalCount, Is.EqualTo(0));
        Assert.That(catResponse.Page, Is.EqualTo(1));
    }

    [Test]
    public async Task ListCats_ShouldReturn_WhenCatsExist()
    {
        // Arrange
        using var scope = _factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var newCat = new CatEntity
        {
            CatId = "cat_123",
            Width = 500,
            Height = 400,
            Image = "/downloads/cat_123.jpg",
            Created = DateTime.UtcNow
        };
        dbContext.Cats.Add(newCat);
        await dbContext.SaveChangesAsync();

        // Act
        var response = await _client.GetAsync("/api/cats?page=1&pageSize=2");

        // Assert
        response.EnsureSuccessStatusCode();
        var responsePayload = await response.Content.ReadAsStringAsync();
        var catResponse = JsonSerializer.Deserialize<ListResponse<CatDto>>(responsePayload, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        });

        Assert.IsNotNull(catResponse);
        Assert.IsNotEmpty(catResponse.Data);
        Assert.That(catResponse.Data.Count(), Is.EqualTo(1));
        Assert.That(catResponse.TotalCount, Is.EqualTo(1));
        Assert.That(catResponse.Page, Is.EqualTo(1));
    }

    [TestCase(10)]
    [TestCase(30)]
    public async Task ListCats_ShouldReturnMax25CatsPerRequest_AtAllTimes(int numOfCats)
    {
        // Arrange
        using var scope = _factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var faker = new Faker();

        var catFaker = new Faker<CatEntity>()
            .RuleFor(c => c.Id, f => f.IndexFaker + 1)
            .RuleFor(c => c.CatId, f => f.Random.AlphaNumeric(10))
            .RuleFor(c => c.Width, f => f.Random.Int(200, 800))
            .RuleFor(c => c.Height, f => f.Random.Int(200, 800))
            .RuleFor(c => c.Image, f => f.Internet.Avatar())
            .RuleFor(c => c.Created, f => f.Date.Past());
        var fakeCats = catFaker.Generate(numOfCats);

        dbContext.Cats.AddRange(fakeCats);
        await dbContext.SaveChangesAsync();

        // Act
        var response = await _client.GetAsync("/api/cats?page=1&pageSize=25");

        // Assert
        response.EnsureSuccessStatusCode();
        var responsePayload = await response.Content.ReadAsStringAsync();
        var catResponse = JsonSerializer.Deserialize<ListResponse<CatDto>>(responsePayload, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        });

        Assert.IsNotNull(catResponse);
        Assert.IsNotEmpty(catResponse.Data);
        Assert.That(catResponse.Data.Count(), Is.LessThanOrEqualTo(numOfCats));
        Assert.That(catResponse.TotalCount, Is.LessThanOrEqualTo(numOfCats));
    }

    [TestCase(0, 10, "cute", HttpStatusCode.BadRequest)]
    [TestCase(-5, 10, "funny", HttpStatusCode.BadRequest)]
    [TestCase(1, 0, "cute", HttpStatusCode.BadRequest)]
    [TestCase(1, -10, "cute", HttpStatusCode.BadRequest)]
    [TestCase(1, 26, "funny", HttpStatusCode.BadRequest)]
    [TestCase(1, 10, "ab", HttpStatusCode.BadRequest)]
    [TestCase(1, 10, "bGZ8Xw3YpLd5h7Qa2KRtcNMFj9VBzuWT6JCXY41sqZmPfxSgA123456ydghis9", HttpStatusCode.BadRequest)]
    [TestCase(1, 10, null, HttpStatusCode.OK)]
    [TestCase(1, 10, "cat", HttpStatusCode.OK)]
    [TestCase(1, 10, "adorable kitty cat", HttpStatusCode.OK)]
    [TestCase(1, 25, "123", HttpStatusCode.OK)]
    public async Task ListCats_CheckValidationRules(int page, int pageSize, string tag, HttpStatusCode expectedStatusCode)
    {
        // Arrange
        using var scope = _factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        var newCat = new CatEntity
        {
            CatId = "cat_123",
            Width = 500,
            Height = 400,
            Image = "/downloads/cat_123.jpg",
            Created = DateTime.UtcNow
        };
        dbContext.Cats.Add(newCat);
        await dbContext.SaveChangesAsync();

        // Act
        var response = await _client.GetAsync($"/api/cats?page={page}&pageSize={pageSize}&tag={tag}");

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(expectedStatusCode));
    }

    [Test]
    public async Task ListCats_ShouldReturnCats_WhenTagMatches()
    {
        // Arrange
        using var scope = _factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var faker = new Faker();

        var availableTags = new List<string> { "cute", "funny", "sleepy", "playful", "grumpy" };
        var catFaker = new Faker<CatEntity>()
            .RuleFor(c => c.Id, f => f.IndexFaker + 1)
            .RuleFor(c => c.CatId, f => f.Random.AlphaNumeric(10))
            .RuleFor(c => c.Width, f => f.Random.Int(200, 800))
            .RuleFor(c => c.Height, f => f.Random.Int(200, 800))
            .RuleFor(c => c.Image, f => f.Internet.Avatar())
            .RuleFor(c => c.Created, f => f.Date.Past())
            .RuleFor(c => c.Tags, f =>
                new List<TagEntity> {
                    new TagEntity { Name = availableTags[0], Created = DateTime.UtcNow },
                    new TagEntity { Name = availableTags[1], Created = DateTime.UtcNow }
                }
            );
        var fakeCats = catFaker.Generate(3);

        dbContext.Cats.AddRange(fakeCats);
        await dbContext.SaveChangesAsync();

        // Act
        var response1 = await _client.GetAsync($"/api/cats?page=1&pageSize=25&tag={availableTags[0]}");
        var response2 = await _client.GetAsync($"/api/cats?page=1&pageSize=25&tag={availableTags[1]}");
        var response3 = await _client.GetAsync($"/api/cats?page=1&pageSize=25&tag={availableTags[2]}");

        // Assert
        response1.EnsureSuccessStatusCode();
        response2.EnsureSuccessStatusCode();
        response3.EnsureSuccessStatusCode();

        var response1Payload = await response1.Content.ReadAsStringAsync();
        var catResponse1 = JsonSerializer.Deserialize<ListResponse<CatDto>>(response1Payload, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        });

        var response2Payload = await response2.Content.ReadAsStringAsync();
        var catResponse2 = JsonSerializer.Deserialize<ListResponse<CatDto>>(response2Payload, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        });

        var response3Payload = await response3.Content.ReadAsStringAsync();
        var catResponse3 = JsonSerializer.Deserialize<ListResponse<CatDto>>(response3Payload, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        });

        Assert.AreEqual(response2Payload, response1Payload);
        Assert.That(catResponse3.Data.Count(), Is.EqualTo(0));
    }

    [Test]
    public async Task ListCats_ShouldReturnCats_WhenTagDoeNotMatch()
    {
        // Arrange
        using var scope = _factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var faker = new Faker();

        var availableTags = new List<string> { "cute", "funny", "sleepy", "playful", "grumpy" };
        var catFaker = new Faker<CatEntity>()
            .RuleFor(c => c.Id, f => f.IndexFaker + 1)
            .RuleFor(c => c.CatId, f => f.Random.AlphaNumeric(10))
            .RuleFor(c => c.Width, f => f.Random.Int(200, 800))
            .RuleFor(c => c.Height, f => f.Random.Int(200, 800))
            .RuleFor(c => c.Image, f => f.Internet.Avatar())
            .RuleFor(c => c.Created, f => f.Date.Past())
            .RuleFor(c => c.Tags, f =>
                new List<TagEntity> {
                    new TagEntity { Name = availableTags[0], Created = DateTime.UtcNow },
                    new TagEntity { Name = availableTags[1], Created = DateTime.UtcNow }
                }
            );
        var fakeCats = catFaker.Generate(3);

        dbContext.Cats.AddRange(fakeCats);
        await dbContext.SaveChangesAsync();

        // Act
        var response3 = await _client.GetAsync($"/api/cats?page=1&pageSize=25&tag={availableTags[2]}");

        // Assert
        response3.EnsureSuccessStatusCode();

        var response3Payload = await response3.Content.ReadAsStringAsync();
        var catResponse3 = JsonSerializer.Deserialize<ListResponse<CatDto>>(response3Payload, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        });

        Assert.That(catResponse3.Data.Count(), Is.EqualTo(0));
    }

    [Test]
    public async Task ListCats_ShouldReturnCats_WhenTagDoeNotMatch1()
    {
        // Arrange
        using var scope = _factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var faker = new Faker();

        var availableTags = new List<string> { "cute", "funny", "sleepy", "playful", "grumpy" };
        var catFaker = new Faker<CatEntity>()
            .RuleFor(c => c.Id, f => f.IndexFaker + 1)
            .RuleFor(c => c.CatId, f => f.Random.AlphaNumeric(10))
            .RuleFor(c => c.Width, f => f.Random.Int(200, 800))
            .RuleFor(c => c.Height, f => f.Random.Int(200, 800))
            .RuleFor(c => c.Image, f => f.Internet.Avatar())
            .RuleFor(c => c.Created, f => f.Date.Past())
            .RuleFor(c => c.Tags, f =>
                new List<TagEntity> {
                    new TagEntity { Name = availableTags[0], Created = DateTime.UtcNow },
                    new TagEntity { Name = availableTags[1], Created = DateTime.UtcNow }
                }
            );
        var fakeCats = catFaker.Generate(3);

        dbContext.Cats.AddRange(fakeCats);
        await dbContext.SaveChangesAsync();

        // Act
        var response3 = await _client.GetAsync($"/api/cats");

        // Assert
        response3.EnsureSuccessStatusCode();

        var response3Payload = await response3.Content.ReadAsStringAsync();
        var catResponse3 = JsonSerializer.Deserialize<ListResponse<CatDto>>(response3Payload, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        });

        Assert.That(catResponse3.Data.Count(), Is.EqualTo(0));
    }
    #endregion

    #region Get Cats Tests
    [Test]
    public async Task GetCatById_ShouldReturnCat_WhenIdMatches()
    {
        // Arrange
        using var scope = _factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var faker = new Faker();

        var catFaker = new Faker<CatEntity>()
            .RuleFor(c => c.Id, f => f.IndexFaker + 1)
            .RuleFor(c => c.CatId, f => f.Random.AlphaNumeric(10))
            .RuleFor(c => c.Width, f => f.Random.Int(200, 800))
            .RuleFor(c => c.Height, f => f.Random.Int(200, 800))
            .RuleFor(c => c.Image, f => f.Internet.Avatar())
            .RuleFor(c => c.Created, f => f.Date.Past());
        var fakeCats = catFaker.Generate(1);

        dbContext.Cats.AddRange(fakeCats);
        await dbContext.SaveChangesAsync();

        // Act
        var response = await _client.GetAsync($"/api/cats/{fakeCats[0].Id}");

        // Assert
        response.EnsureSuccessStatusCode();

        var responsePayload = await response.Content.ReadAsStringAsync();
        var catResponse = JsonSerializer.Deserialize<CatDto>(responsePayload, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        });

        Assert.That(catResponse.Id, Is.EqualTo(fakeCats[0].Id));
        Assert.That(catResponse.Width, Is.EqualTo(fakeCats[0].Width));
        Assert.That(catResponse.Height, Is.EqualTo(fakeCats[0].Height));
        Assert.That(catResponse.ImageUrl, Is.EqualTo(fakeCats[0].Image));
    }

    [Test]
    public async Task GetCatById_ShouldReturnNotFound_WhenIdDoesNotMatch()
    {
        // Arrange
        using var scope = _factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var faker = new Faker();

        var catFaker = new Faker<CatEntity>()
            .RuleFor(c => c.Id, f => f.IndexFaker + 1)
            .RuleFor(c => c.CatId, f => f.Random.AlphaNumeric(10))
            .RuleFor(c => c.Width, f => f.Random.Int(200, 800))
            .RuleFor(c => c.Height, f => f.Random.Int(200, 800))
            .RuleFor(c => c.Image, f => f.Internet.Avatar())
            .RuleFor(c => c.Created, f => f.Date.Past());
        var fakeCats = catFaker.Generate(1);

        dbContext.Cats.AddRange(fakeCats);
        await dbContext.SaveChangesAsync();

        // Act
        var response = await _client.GetAsync($"/api/cats/102920");

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.NotFound));
    }
    #endregion
}
