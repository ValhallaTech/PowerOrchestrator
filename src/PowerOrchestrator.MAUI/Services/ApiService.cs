using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Net.Http.Headers;
using System.Text;

namespace PowerOrchestrator.MAUI.Services;

/// <summary>
/// API service implementation for communicating with the backend
/// </summary>
public class ApiService : IApiService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<ApiService> _logger;
    private readonly IAuthenticationService _authenticationService;

    /// <summary>
    /// Initializes a new instance of the <see cref="ApiService"/> class
    /// </summary>
    /// <param name="httpClient">The HTTP client</param>
    /// <param name="logger">The logger instance</param>
    /// <param name="authenticationService">The authentication service</param>
    public ApiService(
        HttpClient httpClient,
        ILogger<ApiService> logger,
        IAuthenticationService authenticationService)
    {
        _httpClient = httpClient;
        _logger = logger;
        _authenticationService = authenticationService;
        
        // TODO: Configure base address from settings
        _httpClient.BaseAddress = new Uri("https://localhost:7001"); // Default API base URL
        _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
    }

    /// <inheritdoc/>
    public async Task<T?> GetAsync<T>(string endpoint)
    {
        try
        {
            _logger.LogInformation("GET request to: {Endpoint}", endpoint);
            
            AddAuthenticationHeader();
            
            var response = await _httpClient.GetAsync(endpoint);
            
            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<T>(content);
            }
            
            _logger.LogWarning("GET request failed. Status: {StatusCode}, Endpoint: {Endpoint}", response.StatusCode, endpoint);
            return default;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during GET request to: {Endpoint}", endpoint);
            return default;
        }
    }

    /// <inheritdoc/>
    public async Task<T?> PostAsync<T>(string endpoint, object data)
    {
        try
        {
            _logger.LogInformation("POST request to: {Endpoint}", endpoint);
            
            AddAuthenticationHeader();
            
            var json = JsonConvert.SerializeObject(data);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            
            var response = await _httpClient.PostAsync(endpoint, content);
            
            if (response.IsSuccessStatusCode)
            {
                var responseContent = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<T>(responseContent);
            }
            
            _logger.LogWarning("POST request failed. Status: {StatusCode}, Endpoint: {Endpoint}", response.StatusCode, endpoint);
            return default;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during POST request to: {Endpoint}", endpoint);
            return default;
        }
    }

    /// <inheritdoc/>
    public async Task<T?> PutAsync<T>(string endpoint, object data)
    {
        try
        {
            _logger.LogInformation("PUT request to: {Endpoint}", endpoint);
            
            AddAuthenticationHeader();
            
            var json = JsonConvert.SerializeObject(data);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            
            var response = await _httpClient.PutAsync(endpoint, content);
            
            if (response.IsSuccessStatusCode)
            {
                var responseContent = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<T>(responseContent);
            }
            
            _logger.LogWarning("PUT request failed. Status: {StatusCode}, Endpoint: {Endpoint}", response.StatusCode, endpoint);
            return default;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during PUT request to: {Endpoint}", endpoint);
            return default;
        }
    }

    /// <inheritdoc/>
    public async Task<bool> DeleteAsync(string endpoint)
    {
        try
        {
            _logger.LogInformation("DELETE request to: {Endpoint}", endpoint);
            
            AddAuthenticationHeader();
            
            var response = await _httpClient.DeleteAsync(endpoint);
            
            if (response.IsSuccessStatusCode)
            {
                return true;
            }
            
            _logger.LogWarning("DELETE request failed. Status: {StatusCode}, Endpoint: {Endpoint}", response.StatusCode, endpoint);
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during DELETE request to: {Endpoint}", endpoint);
            return false;
        }
    }

    /// <summary>
    /// Adds authentication header to the HTTP client if user is authenticated
    /// </summary>
    private void AddAuthenticationHeader()
    {
        if (_authenticationService.IsAuthenticated && !string.IsNullOrEmpty(_authenticationService.Token))
        {
            _httpClient.DefaultRequestHeaders.Authorization = 
                new AuthenticationHeaderValue("Bearer", _authenticationService.Token);
        }
    }
}