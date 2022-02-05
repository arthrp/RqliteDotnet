using System.Data;
using System.Net;
using System.Text;
using System.Text.Json;
using RqliteDotnet.Dto;

namespace RqliteDotnet;

public class RqliteClient
{
    private readonly HttpClient _httpClient;

    public RqliteClient(string uri, HttpClient? client = null)
    {
        _httpClient = client ?? new HttpClient(){ BaseAddress = new Uri(uri) };
    }

    public async Task<string> Ping()
    {
        var x = await _httpClient.GetAsync("/status");

        return x.Headers.GetValues("X-Rqlite-Version").FirstOrDefault();
    }

    public async Task<QueryResults> Query(string query, bool getTimings = false)
    {
        var data = "&q="+Uri.EscapeDataString(query);
        var baseUrl = "/db/query?pretty";

        baseUrl = (getTimings) ? $"{baseUrl}&timings" : baseUrl;

        var r = await _httpClient.GetAsync($"{baseUrl}&{data}");
        var str = await r.Content.ReadAsStringAsync();

        var result = JsonSerializer.Deserialize<QueryResults>(str, new JsonSerializerOptions() { PropertyNameCaseInsensitive = true });
        return result;
    }

    public async Task<ExecuteResults> Execute(string command)
    {
        var request = new HttpRequestMessage(HttpMethod.Post, "/db/execute?pretty&timings");
        request.Content = new StringContent($"[\"{command}\"]", Encoding.UTF8, "application/json");

        var response = await _httpClient.SendAsync(request);
        var content = await response.Content.ReadAsStringAsync();

        var result = JsonSerializer.Deserialize<ExecuteResults>(content, new JsonSerializerOptions() { PropertyNameCaseInsensitive = true });
        return result;
    }

    public async Task<T> Query<T>(string query, bool getTimings = true) where T: new()
    {
        var response = await Query(query, getTimings);
        if (response.Results.Count > 1)
            throw new DataException("Query returned more than 1 result. At the moment only 1 result supported");
        if (!string.IsNullOrEmpty(response.Results[0].Error))
            throw new InvalidOperationException(response.Results[0].Error);
        var result = new T();

        foreach (var prop in typeof(T).GetProperties())
        {
            var index = response.Results[0].Columns.FindIndex(c => c.ToLower() == prop.Name.ToLower());
            var typ = prop.PropertyType;
            var x = GetValue(response.Results[0].Types[index], response.Results[0].Values[0][index]);
            
            prop.SetValue(result, x);
        }
        
        return result;
    }

    private object GetValue(string valType, JsonElement el)
    {
        object? x = valType switch
        {
            "text" => el.GetString(),
            "integer" or "numeric" => el.GetInt32(),
            "real" => el.GetDouble(),
            _ => throw new ArgumentException("Unsupported type")
        };

        return x;
    }
}