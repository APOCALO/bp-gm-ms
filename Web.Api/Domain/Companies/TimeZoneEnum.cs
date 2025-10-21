namespace Web.Api.Domain.Companies
{
    public enum TimeZoneEnum
    {
        // UTC base
        UTC = 0,

        // América
        America_Bogota = 1,  // Colombia
        America_MexicoCity = 2,  // México
        America_New_York = 3,  // USA - Este
        America_Los_Angeles = 4,  // USA - Pacífico
        America_Buenos_Aires = 5,  // Argentina

        // Europa
        Europe_London = 10,
        Europe_Paris = 11,
        Europe_Madrid = 12,
        Europe_Berlin = 13,

        // Asia
        Asia_Tokyo = 20,
        Asia_Shanghai = 21,
        Asia_Dubai = 22,
        Asia_Singapore = 23
    }
}
