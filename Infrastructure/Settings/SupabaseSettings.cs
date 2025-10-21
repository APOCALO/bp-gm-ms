namespace Infrastructure.Settings
{
    public class SupabaseSettings
    {
        public string ProjectRef { get; set; } = string.Empty;
        public string? JwtSecret { get; set; }
        public string? Url { get; set; } // si usas custom domain, puedes setearlo aquí
        public string? ServiceRoleKey { get; set; }
        public string? AnonKey { get; set; }

        public string ResolvedUrl =>
            !string.IsNullOrWhiteSpace(Url) ? Url! : $"https://{ProjectRef}.supabase.co";
    }
}
