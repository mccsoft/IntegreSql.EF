using System.Net.Http;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.VisualStudio.TestPlatform.TestHost;

namespace ExampleWeb.IntegrationTests;

public class IntegrationTestBase
{
    protected readonly HttpClient _httpClient;

    public IntegrationTestBase()
    {
        var webAppFactory = new WebApplicationFactory<Program>();

        _httpClient = webAppFactory.CreateDefaultClient();
    }
}
