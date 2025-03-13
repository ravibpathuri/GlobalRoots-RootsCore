using Neo4j.Driver;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Collections;
using System.Linq;

namespace RootsCore.Tests;

[TestFixture]
public class PromptControllerTests
{
    private Mock<Neo4jContext> _neo4jMock;
    private Mock<IAiService> _aiServiceMock;
    private PromptController _controller;

    [SetUp]
    public void Setup()
    {
        // Setup mocks
        _neo4jMock = new Mock<Neo4jContext>(MockBehavior.Strict);
        _neo4jMock.SetupGet(n => n.IsConnected).Returns(true);
        _neo4jMock.Setup(n => n.Session).Returns(Mock.Of<IAsyncSession>());

        _aiServiceMock = new Mock<IAiService>(MockBehavior.Strict);

        _controller = new PromptController(_neo4jMock.Object, _aiServiceMock.Object);
    }

    [Test]
    public async Task GeneratePrompt_ValidInput_ReturnsOk()
    {
        // Arrange
        var request = new GenerateRequest { Context = "Anyone in Sydney, Australia, from the Bondi colony related to my spouse’s brother’s family" };
        _aiServiceMock
            .Setup(s => s.GeneratePromptAsync(request.Context))
            .ReturnsAsync("Who in Bondi connects to my spouse’s brother?");

        // Act
        var result = await _controller.GeneratePrompt(request);

        // Assert
        Assert.That(result, Is.InstanceOf<OkObjectResult>());

        var okResult = (OkObjectResult)result;
        dynamic response = okResult.Value;
        Assert.That(response.prompt, Is.EqualTo("Who in Bondi connects to my spouse’s brother?"));
    }

    [Test]
    public async Task GeneratePrompt_NoDatabase_Returns503()
    {
        // Arrange
        _neo4jMock.SetupGet(n => n.IsConnected).Returns(false);
        var request = new GenerateRequest { Context = "Test context" };

        // Act
        var result = await _controller.GeneratePrompt(request);

        // Assert

        Assert.That(result, Is.InstanceOf<ObjectResult>());
        var statusResult = (ObjectResult)result;
        Assert.That(statusResult.StatusCode, Is.EqualTo(503));
        Assert.That(statusResult.Value, Is.EqualTo("Database unavailable"));
    }

    [Test]
    public async Task SuggestPrompts_ValidData_ReturnsOk()
    {
        // Arrange
        var mockSession = new Mock<IAsyncSession>();
        var mockResult = new Mock<IResultCursor>();

        // Fix: Use Query object instead of string
        mockSession.Setup(s => s.RunAsync(It.IsAny<string>(), It.IsAny<object>())).ReturnsAsync(mockResult.Object);

        // Mock the result cursor to simulate Neo4j data
        mockResult.Setup(r => r.ForEachAsync(It.IsAny<Action<IRecord>>()))
            .Callback<Action<IRecord>>(action =>
            {
                action(new MockRecord(new Dictionary<string, object>
                {
                    { "missing_birth", "John Doe has no birth date" },
                    { "missing_children", null }
                }));
            });

        _neo4jMock.Setup(n => n.Session).Returns(mockSession.Object);
        _aiServiceMock.Setup(s => s
            .SuggestPromptsAsync(It.Is<List<string>>(l => l.Contains("John Doe has no birth date"))))
            .ReturnsAsync(new List<string> { "When was John Doe born?" });

        // Act
        var result = await _controller.SuggestPrompts();

        // Assert

        Assert.That(result, Is.InstanceOf<OkObjectResult>());
        var okResult = (OkObjectResult)result;
        dynamic response = okResult.Value;
        var suggestions = (List<string>)response.suggestions;
        Assert.That(suggestions, Is.AnyOf("When was John Doe born?"));
    }

    [Test]
    public async Task SuggestPrompts_NoDatabase_Returns503()
    {
        // Arrange
        _neo4jMock.SetupGet(n => n.IsConnected).Returns(false);

        // Act
        var result = await _controller.SuggestPrompts();

        // Assert
        Assert.That(result, Is.InstanceOf<OkObjectResult>());
        var statusResult = (ObjectResult)result;

        Assert.That(statusResult.StatusCode, Is.EqualTo(503));
        Assert.That(statusResult.Value, Is.EqualTo("Database unavailable"));
    }
}

// Helper class to mock IRecord
public class MockRecord : IRecord
{
    private readonly IDictionary<string, object> _data;

    public MockRecord(IDictionary<string, object> data)
    {
        _data = data;
    }

    public object this[string key] => _data[key];

    public object this[int index] => throw new NotImplementedException();

    public IReadOnlyDictionary<string, object> Values => (IReadOnlyDictionary<string, object>)_data;
    public IReadOnlyList<string> Keys => _data.Keys.ToList().AsReadOnly();

    public int Count => throw new NotImplementedException();

    IEnumerable<string> IReadOnlyDictionary<string, object>.Keys => throw new NotImplementedException();

    IEnumerable<object> IReadOnlyDictionary<string, object>.Values => throw new NotImplementedException();

    public bool ContainsKey(string key)
    {
        throw new NotImplementedException();
    }

    public T Get<T>(string key)
    {
        throw new NotImplementedException();
    }

    public T GetCaseInsensitive<T>(string key)
    {
        throw new NotImplementedException();
    }

    public IEnumerator<KeyValuePair<string, object>> GetEnumerator()
    {
        throw new NotImplementedException();
    }

    public bool TryGet<T>(string key, out T value)
    {
        throw new NotImplementedException();
    }

    public bool TryGetCaseInsensitive<T>(string key, out T value)
    {
        throw new NotImplementedException();
    }

    public bool TryGetValue(string key, [MaybeNullWhen(false)] out object value)
    {
        throw new NotImplementedException();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        throw new NotImplementedException();
    }
}