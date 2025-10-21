namespace Application.Interfaces
{
    public interface ISupabaseService
    {
        Task<string> GetAsync(string path, string? userAccessToken = null);
        Task<string> PostAsync(string path, object body, string? userAccessToken = null);
        Task<string> PutAsync(string path, object body, string? userAccessToken = null);
        Task<string> DeleteAsync(string path, string? userAccessToken = null);
    }
}
