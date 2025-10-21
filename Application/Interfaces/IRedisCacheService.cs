namespace Application.Interfaces
{
    public interface IRedisCacheService
    {
        Task SetAsync<T>(string key, T value, TimeSpan? expiry = null);
        Task<T?> GetAsync<T>(string key);
        Task RemoveAsync(string key);
        // método para obtener múltiples valores en un solo roundtrip
        Task<Dictionary<string, T?>> GetManyAsync<T>(IEnumerable<string> keys);
    }
}
