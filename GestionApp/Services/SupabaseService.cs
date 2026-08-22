using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;

namespace GestionApp.Services;

/// <summary>
/// Talks to the same Supabase project as the gestion-app website (PostgREST
/// over HTTPS), using the same public "publishable" key. Safe to embed
/// because Row Level Security policies, not key secrecy, gate access —
/// matches the SUPABASE_URL / SUPABASE_KEY constants in gestion-app.html.
/// </summary>
public class SupabaseService
{
    private const string BaseUrl = "https://fquwpdzceuqlxvewacxz.supabase.co/rest/v1/";
    private const string Key = "sb_publishable_M6FUId2WUSj6tidr0rGeeQ_h6Qt0YFD";

    private readonly HttpClient _http;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
    };

    public SupabaseService()
    {
        _http = new HttpClient { BaseAddress = new Uri(BaseUrl) };
        _http.DefaultRequestHeaders.Add("apikey", Key);
        _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", Key);
    }

    public async Task<List<T>> GetAllAsync<T>(string table, string query = "select=*&order=created_at.asc")
    {
        var result = await _http.GetFromJsonAsync<List<T>>($"{table}?{query}");
        return result ?? new List<T>();
    }

    public async Task InsertAsync<T>(string table, T row)
    {
        using var req = new HttpRequestMessage(HttpMethod.Post, table)
        {
            Content = JsonContent.Create(row, options: JsonOptions)
        };
        req.Headers.Add("Prefer", "return=minimal");
        var res = await _http.SendAsync(req);
        if (!res.IsSuccessStatusCode)
            throw new Exception($"INSERT {table} a échoué ({(int)res.StatusCode}) : {await res.Content.ReadAsStringAsync()}");
    }

    public async Task UpdateAsync<T>(string table, string id, T patch)
    {
        using var req = new HttpRequestMessage(HttpMethod.Patch, $"{table}?id=eq.{Uri.EscapeDataString(id)}")
        {
            Content = JsonContent.Create(patch, options: JsonOptions)
        };
        req.Headers.Add("Prefer", "return=minimal");
        var res = await _http.SendAsync(req);
        if (!res.IsSuccessStatusCode)
            throw new Exception($"PATCH {table} a échoué ({(int)res.StatusCode}) : {await res.Content.ReadAsStringAsync()}");
    }

    public async Task DeleteAsync(string table, string id)
    {
        var res = await _http.DeleteAsync($"{table}?id=eq.{Uri.EscapeDataString(id)}");
        if (!res.IsSuccessStatusCode)
            throw new Exception($"DELETE {table} a échoué ({(int)res.StatusCode})");
    }
}
