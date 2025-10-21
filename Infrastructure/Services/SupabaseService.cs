using Application.Interfaces;
using Infrastructure.Settings;
using System.Net.Http.Json;

namespace Infrastructure.Services
{
    public class SupabaseService : ISupabaseService
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly SupabaseSettings _settings;

        public SupabaseService(IHttpClientFactory httpClientFactory, SupabaseSettings settings)
        {
            _httpClientFactory = httpClientFactory;
            _settings = settings;
        }

        public async Task<string> GetAsync(string path, string? userAccessToken = null)
        {
            var client = CreateClient(userAccessToken);
            var res = await client.GetAsync(path);
            res.EnsureSuccessStatusCode();
            return await res.Content.ReadAsStringAsync();
        }

        public async Task<string> PostAsync(string path, object body, string? userAccessToken = null)
        {
            var client = CreateClient(userAccessToken);
            var res = await client.PostAsJsonAsync(path, body);
            res.EnsureSuccessStatusCode();
            return await res.Content.ReadAsStringAsync();
        }

        public async Task<string> PutAsync(string path, object body, string? userAccessToken = null)
        {
            var client = CreateClient(userAccessToken);
            var res = await client.PutAsJsonAsync(path, body);
            res.EnsureSuccessStatusCode();
            return await res.Content.ReadAsStringAsync();
        }

        public async Task<string> DeleteAsync(string path, string? userAccessToken = null)
        {
            var client = CreateClient(userAccessToken);
            var res = await client.DeleteAsync(path);
            res.EnsureSuccessStatusCode();
            return await res.Content.ReadAsStringAsync();
        }

        private HttpClient CreateClient(string? userAccessToken)
        {
            var client = _httpClientFactory.CreateClient("Supabase");

            // Si quieres actuar en nombre del usuario, reemplaza la auth header
            if (!string.IsNullOrEmpty(userAccessToken))
            {
                client.DefaultRequestHeaders.Remove("Authorization");
                client.DefaultRequestHeaders.Add("Authorization", $"Bearer {userAccessToken}");
            }

            return client;
        }
    }
}
