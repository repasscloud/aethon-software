namespace Aethon.Shared.Enums;

public enum JobCategory : int
{
    // 100 - Business, finance, and office
    Accounting = 101,
    AdminSecretarial = 102,
    Banking = 103,
    ExecutiveManagement = 104,
    FinanceInsurance = 105,
    HumanResources = 106,
    LegalServices = 107,
    Recruitment = 108,

    // 200 - Marketing, media, and creative
    AdvertisingPR = 201,
    Arts = 202,
    Design = 203,
    Marketing = 204,
    MediaJournalism = 205,
    PublicRelations = 206,

    // 300 - Technology and telecommunications
    ITSoftware = 301,
    TelecommunicationsISP = 302,

    // 400 - Engineering, science, and technical
    Aerospace = 401,
    Engineering = 402,
    Research = 403,
    Science = 404,
    SecurityIntelligence = 405,

    // 500 - Primary industries and environment
    AgricultureFishingForestry = 501,
    MiningResources = 502,
    UtilitiesEnergy = 503,
    Veterinary = 504,

    // 600 - Construction, property, and infrastructure
    BuildingConstruction = 601,
    PropertyRealEstate = 602,

    // 700 - Manufacturing, transport, and logistics
    Automobile = 701,
    Logistics = 702,
    Manufacturing = 703,
    TransportDistribution = 704,

    // 800 - Government, education, and public services
    Education = 801,
    Government = 802,
    GraduateRoles = 803,
    SocialWork = 804,
    Charity = 805,

    // 900 - Health and care
    Healthcare = 901,
    Pharmaceuticals = 902,

    // 1000 - Hospitality, tourism, and service
    Catering = 1001,
    CustomerService = 1002,
    FoodBeverage = 1003,
    Hospitality = 1004,
    Tourism = 1005,

    // 1100 - Retail, sales, and consumer
    Retail = 1101,
    Sales = 1102,

    // 1200 - Lifestyle, recreation, and flexible work
    PartTimeTemp = 1201,
    SportRecreation = 1202,

    // 1300 - Misc
    Other = 1301
}