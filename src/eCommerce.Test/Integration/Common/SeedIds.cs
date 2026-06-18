namespace eCommerce.Test.Integration.Common;

/// <summary>
/// Well-known identifiers seeded into the database via the persistence layer's HasData configuration.
/// <para>
/// The production seed class (<c>AppDbSeedData</c>) is internal, so the same GUIDs are mirrored here to
/// let tests arrange against existing rows (e.g. order a real product, review a real product) without
/// having to create that data first.
/// </para>
/// </summary>
public static class SeedIds
{
    // Users seeded by the persistence layer.
    public static readonly Guid ReviewerOneId = Guid.Parse("8ad8a683-a2c1-4df2-94af-b1b4470cb6d1"); // Lena Carter
    public static readonly Guid ReviewerTwoId = Guid.Parse("bcbd8034-46f1-4e07-9f45-4b3871572a4b"); // Marcus Reed

    // Categories.
    public static readonly Guid ElectronicsCategoryId = Guid.Parse("28978b4b-df88-4c5a-b371-d4c10eaebff9");
    public static readonly Guid HomeOfficeCategoryId = Guid.Parse("c6f81854-315d-4676-a221-7b578d9d0f44");
    public static readonly Guid FitnessCategoryId = Guid.Parse("4b4fa4f4-6905-4036-808f-c5876c469bb1");

    // Products (all have stock, so they are safe to order in tests).
    public static readonly Guid NoiseCancellingHeadphonesId = Guid.Parse("fb7a7720-c40a-4eef-bb5e-9a592b6e00c5");
    public static readonly Guid MechanicalKeyboardId = Guid.Parse("db6f5fd4-bf4e-44b9-88b9-6df6cfd0a13e");
    public static readonly Guid ErgonomicChairId = Guid.Parse("bc7f9c55-ad74-448f-870b-13b8b82f501c");
    public static readonly Guid StandingDeskId = Guid.Parse("7593a8b6-c0ae-4b0b-b350-37deaf436fc2");
    public static readonly Guid SmartWatchId = Guid.Parse("6ea52fd5-f353-42d0-af5e-e09a7997fa8a");
    public static readonly Guid YogaMatId = Guid.Parse("2dd712d1-7cee-4b47-987a-446eb1907f6a");

    // A reviewed product (Lena reviewed the headphones), useful for "get reviews by product" tests.
    public static readonly Guid ReviewedProductId = NoiseCancellingHeadphonesId;

    // An id guaranteed not to exist, for 404 tests.
    public static readonly Guid NonExistentId = Guid.Parse("00000000-0000-0000-0000-0000000000ff");

    public const string ElectronicsCategoryName = "Electronics";
}
