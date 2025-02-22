namespace Natech.Caas.Core.UnitTests;

using Moq;
using Natech.Caas.Core.Services;
using Natech.Caas.Database.Repository;
using Natech.Caas.TheCatApi.Client;
using System.Collections.Generic;
using System.Threading.Tasks;
using NUnit.Framework;
using Microsoft.Extensions.Configuration;
using Natech.Caas.TheCatApi.Dtos;
using Natech.Caas.Database.Entities;

public class CatServiceTests
{
    private Mock<IConfiguration> _mockConfiguration;
    private Mock<ICatRepository> _mockCatRepository;
    private Mock<ITagRepository> _mockTagRepository;
    private Mock<ITheCatApiClient> _mockCatApiClient;
    private Mock<IDownloader> _mockDownloader;
    private CatService _SUT;
    private string _testHost = "http://localhost";

    [SetUp]
    public void Setup()
    {
        _mockConfiguration = new Mock<IConfiguration>();
        _mockConfiguration.Setup(config => config.GetSection("Kestrel:Endpoints:Http:Url").Value)
            .Returns(_testHost);

        _mockCatApiClient = new Mock<ITheCatApiClient>();
        _mockDownloader = new Mock<IDownloader>();
        _mockCatRepository = new Mock<ICatRepository>();
        _mockTagRepository = new Mock<ITagRepository>();

        _SUT = new CatService(
            _mockConfiguration.Object,
            _mockCatRepository.Object,
            _mockTagRepository.Object,
            _mockCatApiClient.Object,
            _mockDownloader.Object
        );
    }

    [Test]
    public async Task SaveCats_ShouldCall_GetRandomCatImagesAsync()
    {
        // Arrange
        var fakeCats = new List<CatImage>
        {
            new CatImage
            {
                Id = "cat1",
                Width = 300,
                Height = 400,
                Url = "https://catapi.com/cat1.jpg",
                Breeds = new List<Breed>
                {
                    new Breed { Id = "", Temperament = "tag1, tag2, tag3" }
                }
            }
        };

        _mockCatApiClient.Setup(api => api.GetRandomCatImagesAsync(It.IsAny<Int32>())).ReturnsAsync(fakeCats);

        // Act
        await _SUT.SaveCats();

        // Assert
        _mockCatApiClient.Verify(api => api.GetRandomCatImagesAsync(It.IsAny<Int32>()), Times.Once);
    }

    [Test]
    public async Task SaveCats_ShouldDownloadImage()
    {
        // Arrange
        var fakeCats = new List<CatImage>
        {
            new CatImage
            {
                Id = "cat1",
                Width = 300,
                Height = 400,
                Url = "https://catapi.com/cat1.jpg",
                Breeds = new List<Breed>
                {
                    new Breed { Id = "", Temperament = "tag1, tag2" }
                }
            }
        };

        _mockCatApiClient.Setup(api => api.GetRandomCatImagesAsync(It.IsAny<Int32>())).ReturnsAsync(fakeCats);

        // Act
        await _SUT.SaveCats();

        // Assert
        _mockDownloader.Verify(d => d.Download(It.IsAny<string>(), It.IsAny<string>()), Times.Once);
    }

    [Test]
    public async Task SaveCats_ShouldSaveNewCatEntity()
    {
        // Arrange
        var fakeCats = new List<CatImage>
        {
            new CatImage
            {
                Id = "cat1",
                Width = 300,
                Height = 400,
                Url = "https://catapi.com/cat1.jpg",
                Breeds = new List<Breed>
                {
                    new Breed { Id = "", Temperament = "tag1, tag2" }
                }
            }
        };

        _mockCatApiClient.Setup(api => api.GetRandomCatImagesAsync(It.IsAny<Int32>())).ReturnsAsync(fakeCats);

        // Act
        await _SUT.SaveCats();

        // Assert
        _mockCatRepository.Verify(repo => repo.Add(It.IsAny<CatEntity>()), Times.Once);
    }

    [Test]
    public async Task SaveCats_ShouldAvoidDuplicateTags()
    {
        // Arrange
        var fakeCats = new List<CatImage>
        {
            new CatImage
            {
                Id = "cat1",
                Width = 300,
                Height = 400,
                Url = "https://catapi.com/cat1.jpg",
                Breeds = new List<Breed>
                {
                    new Breed { Id = "breed1", Temperament = "tag1, tag1" }
                }
            }
        };

        var existingTags = new List<TagEntity>
        {
            new TagEntity { Name = "tag1", Created = DateTime.UtcNow }
        };

        _mockCatApiClient.Setup(api => api.GetRandomCatImagesAsync(It.IsAny<Int32>())).ReturnsAsync(fakeCats);
        _mockTagRepository.Setup(repo => repo.AddAll(It.IsAny<IEnumerable<TagEntity>>())).Returns(Task.CompletedTask);
        _mockTagRepository.Setup(repo => repo.List(It.IsAny<List<string>>())).ReturnsAsync(existingTags);

        // Act
        await _SUT.SaveCats();

        // Assert
        _mockTagRepository.Verify(repo =>
            repo.AddAll(It.IsAny<IEnumerable<TagEntity>>()), Times.Once);
    }

    [Test]
    public async Task SaveCats_ShouldAssignTagsToCat()
    {
        // Arrange
        var fakeCats = new List<CatImage>
        {
            new CatImage
            {
                Id = "cat1",
                Width = 300,
                Height = 400,
                Url = "https://catapi.com/cat1.jpg",
                Breeds = new List<Breed>
                {
                    new Breed { Id = "breed1", Temperament = "tag1, tag2" }
                }
            }
        };

        var existingTags = new List<TagEntity>
        {
            new TagEntity { Name = "tag1", Created = DateTime.UtcNow }
        };

        _mockCatApiClient.Setup(api => api.GetRandomCatImagesAsync(It.IsAny<Int32>())).ReturnsAsync(fakeCats);
        _mockTagRepository.Setup(repo => repo.List(It.IsAny<List<string>>())).ReturnsAsync(existingTags);

        // Act
        await _SUT.SaveCats();

        // Assert
        _mockCatRepository.Verify(repo =>
            repo.Update(It.Is<CatEntity>(c => c.Tags.Count == 2)), Times.Once);
    }

    [Test]
    public async Task GetCat_ShouldReturnNull_WhenCatDoesNotExist()
    {
        // Arrange
        _mockCatRepository.Setup(repo => repo.GetByIdAsync(It.IsAny<int>()))
            .ReturnsAsync((CatEntity)null);

        // Act
        var result = await _SUT.GetCat(999);

        // Assert
        Assert.That(result, Is.Null);
    }

    [Test]
    public async Task GetCat_ShouldReturnCatDto_WhenCatExists()
    {
        // Arrange
        var fakeCat = new CatEntity
        {
            Id = 1,
            CatId = "cat1",
            Width = 300,
            Height = 400,
            Image = "img1.jpg",
            Created = DateTime.UtcNow,
            Tags = new List<TagEntity>
            {
                new TagEntity() { Id = 1, Name = "tag1" },
                new TagEntity() { Id = 2, Name = "tag2" }
            }
        };

        _mockCatRepository.Setup(repo => repo.GetByIdAsync(1))
            .ReturnsAsync(fakeCat);

        // Act
        var result = await _SUT.GetCat(1);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.Width, Is.EqualTo(fakeCat.Width));
        Assert.That(result.ImageUrl, Is.EqualTo(fakeCat.Image));
        Assert.That(result.Tags, Is.EquivalentTo(new List<string>
        {
            "tag1", "tag2"
        }));
    }
}
