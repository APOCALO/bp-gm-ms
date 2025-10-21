namespace Web.Api.Domain.Common
{
    public enum TimeZoneEnum
    {
        // UTC base
        UTC = 0,

        // América Latina
        America_Bogota = 1,  // Colombia, Ecuador, Panamá, Perú
        America_MexicoCity = 2,  // México (Centro)
        America_Buenos_Aires = 5,  // Argentina
        America_Santiago = 6,  // Chile (Continental)
        America_Lima = 7,  // Perú (alternativo, mismo offset que Bogotá)
        America_Caracas = 8,  // Venezuela
        America_La_Paz = 9,  // Bolivia
        America_Montevideo = 50,  // Uruguay
        America_Asuncion = 51,  // Paraguay

        // Brasil (múltiples zonas horarias)
        America_Sao_Paulo = 30,  // Brasil - Sudeste/Sur (Brasília Time)
        America_Manaus = 31,  // Brasil - Amazonas (Amazon Time)
        America_Rio_Branco = 32,  // Brasil - Acre
        America_Noronha = 33,  // Brasil - Fernando de Noronha

        // Centroamérica y Caribe
        America_Guatemala = 40,  // Guatemala, El Salvador, Costa Rica, Honduras, Nicaragua
        America_Tegucigalpa = 41,  // Honduras (alternativo)
        America_San_Jose = 42,  // Costa Rica (alternativo)
        America_Panama = 43,  // Panamá (alternativo, mismo offset que Bogotá)
        America_Havana = 44,  // Cuba
        America_Santo_Domingo = 45,  // República Dominicana
        America_Port_au_Prince = 46,  // Haití
        America_Managua = 47,  // Nicaragua (alternativo)

        // Otros países de Sudamérica
        America_Cayenne = 53,  // Guayana Francesa
        America_Paramaribo = 54,  // Surinam
        America_Guyana = 55,  // Guyana

        // México (zonas adicionales)
        America_Cancun = 60,  // México - Quintana Roo (Este)
        America_Tijuana = 61,  // México - Pacífico
        America_Chihuahua = 62,  // México - Montaña
        America_Mazatlan = 63,  // México - Pacífico (Sinaloa)

        // USA
        America_New_York = 3,  // USA - Este
        America_Los_Angeles = 4,  // USA - Pacífico

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
