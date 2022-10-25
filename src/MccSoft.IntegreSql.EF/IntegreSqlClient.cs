using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using MccSoft.IntegreSql.EF.Dto;
using MccSoft.IntegreSql.EF.Exceptions;
using Microsoft.Extensions.Logging;

namespace MccSoft.IntegreSql.EF;

public class IntegreSqlClient
{
    private readonly ILogger<IntegreSqlClient> _logger;
    private readonly HttpClient _httpClient;

    /// <summary>
    /// Initializes IntegreSQL Client
    /// </summary>
    /// <param name="integreUri">
    /// IntegreURI, e.g. 'http://localhost:5000/api/v1/'.
    /// 'api/v1' should be included in the URI!
    /// </param>
    /// <param name="logger">Logger</param>
    public IntegreSqlClient(Uri integreUri, ILogger<IntegreSqlClient> logger = null)
    {
        _logger = logger;
        _httpClient = new HttpClient { BaseAddress = integreUri, };
    }

    /// <summary>
    /// Initializes IntegreSQL Client
    /// </summary>
    /// <param name="httpClient">
    /// HttpClient which will be used to send requests to IntegreSQL.
    /// Make sure it's BaseAddress is set to URI of running IntegreSQL server!
    /// </param>
    /// <param name="logger">Logger</param>
    public IntegreSqlClient(HttpClient httpClient, ILogger<IntegreSqlClient> logger = null)
    {
        _httpClient = httpClient;
    }

    /// <summary>
    /// Returns NULL if template database is already created/is being created.
    /// </summary>
    public async Task<CreateTemplateDto> InitializeTemplate(string hash)
    {
        HttpResponseMessage response;
        try
        {
            response = await _httpClient
                .PostAsJsonAsync("templates", new { hash = hash })
                .ConfigureAwait(false);
        }
        catch (HttpRequestException e)
        {
            throw new IntegreSqlNotRunningException(_httpClient.BaseAddress?.ToString(), e);
        }

        if (response.IsSuccessStatusCode)
        {
            return await response.Content
                .ReadFromJsonAsync<CreateTemplateDto>()
                .ConfigureAwait(false);
        }

        if (response.StatusCode == HttpStatusCode.Locked)
            return null;

        if (
            response.StatusCode == HttpStatusCode.ServiceUnavailable
            || response.StatusCode == HttpStatusCode.InternalServerError
        )
        {
            throw new IntegreSqlPostgresNotAvailableException(_httpClient.BaseAddress!.ToString());
        }

        response.EnsureSuccessStatusCode();

        throw new NotImplementedException("We should never reach this point");
    }

    public async Task FinalizeTemplate(string hash)
    {
        HttpResponseMessage response;
        try
        {
            response = await _httpClient.PutAsync($"templates/{hash}", null).ConfigureAwait(false);
        }
        catch (HttpRequestException e)
        {
            throw new IntegreSqlNotRunningException(_httpClient.BaseAddress?.ToString(), e);
        }

        if (response.IsSuccessStatusCode)
            return;

        if (response.StatusCode == HttpStatusCode.NotFound)
        {
            throw new IntegreSqlTemplateNotFoundException(hash);
        }
        if (response.StatusCode == HttpStatusCode.ServiceUnavailable)
        {
            throw new IntegreSqlPostgresNotAvailableException(_httpClient.BaseAddress?.ToString());
        }

        response.EnsureSuccessStatusCode();
        throw new NotImplementedException("We should never reach this point");
    }

    public async Task DiscardTemplate(string hash)
    {
        HttpResponseMessage response;
        try
        {
            response = await _httpClient.DeleteAsync($"templates/{hash}").ConfigureAwait(false);
        }
        catch (HttpRequestException e)
        {
            throw new IntegreSqlNotRunningException(_httpClient.BaseAddress?.ToString(), e);
        }

        if (response.IsSuccessStatusCode)
            return;

        if (response.StatusCode == HttpStatusCode.NotFound)
        {
            throw new IntegreSqlTemplateNotFoundException(hash);
        }
        if (response.StatusCode == HttpStatusCode.ServiceUnavailable)
        {
            throw new IntegreSqlPostgresNotAvailableException(_httpClient.BaseAddress?.ToString());
        }

        response.EnsureSuccessStatusCode();
        throw new NotImplementedException("We should never reach this point");
    }

    public async Task<GetDatabaseDto> GetTestDatabase(string hash)
    {
        HttpResponseMessage response;
        try
        {
            response = await _httpClient.GetAsync($"templates/{hash}/tests").ConfigureAwait(false);
        }
        catch (HttpRequestException e)
        {
            throw new IntegreSqlNotRunningException(_httpClient.BaseAddress?.ToString(), e);
        }

        if (response.IsSuccessStatusCode)
        {
            return await response.Content.ReadFromJsonAsync<GetDatabaseDto>();
        }

        switch (response.StatusCode)
        {
            case HttpStatusCode.NotFound:
                throw new IntegreSqlTemplateNotFoundException(hash);

            case HttpStatusCode.ServiceUnavailable:
                throw new IntegreSqlPostgresNotAvailableException(
                    _httpClient.BaseAddress?.ToString()
                );

            case HttpStatusCode.Gone:
                throw new IntegreSqlTemplateDiscardedException(hash);

            case HttpStatusCode.InternalServerError:
                var content = await response.Content.ReadAsStringAsync();
                throw new IntegreSqlInternalServerErrorException(content);
        }

        response.EnsureSuccessStatusCode();

        throw new NotImplementedException("We should never reach this point");
    }

    public async Task ReturnTestDatabase(string hash, int id)
    {
        HttpResponseMessage response;
        try
        {
            response = await _httpClient
                .DeleteAsync($"templates/{hash}/tests/{id}")
                .ConfigureAwait(false);
        }
        catch (HttpRequestException e)
        {
            throw new IntegreSqlNotRunningException(_httpClient.BaseAddress?.ToString(), e);
        }

        if (response.IsSuccessStatusCode)
            return;

        if (response.StatusCode == HttpStatusCode.NotFound)
        {
            throw new IntegreSqlTemplateNotFoundException(hash);
        }
        if (response.StatusCode == HttpStatusCode.ServiceUnavailable)
        {
            throw new IntegreSqlPostgresNotAvailableException(_httpClient.BaseAddress?.ToString());
        }

        response.EnsureSuccessStatusCode();
        throw new NotImplementedException("We should never reach this point");
    }
}
