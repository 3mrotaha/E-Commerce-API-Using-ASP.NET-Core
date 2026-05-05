namespace eCommerce.Persistence.Data;

internal static class AppDbSeedData
{
    internal static readonly Guid ReviewerOneId = Guid.Parse("8ad8a683-a2c1-4df2-94af-b1b4470cb6d1");
    internal static readonly Guid ReviewerTwoId = Guid.Parse("bcbd8034-46f1-4e07-9f45-4b3871572a4b");

    internal static readonly Guid ElectronicsCategoryId = Guid.Parse("28978b4b-df88-4c5a-b371-d4c10eaebff9");
    internal static readonly Guid HomeOfficeCategoryId = Guid.Parse("c6f81854-315d-4676-a221-7b578d9d0f44");
    internal static readonly Guid FitnessCategoryId = Guid.Parse("4b4fa4f4-6905-4036-808f-c5876c469bb1");

    internal static readonly Guid NoiseCancellingHeadphonesId = Guid.Parse("fb7a7720-c40a-4eef-bb5e-9a592b6e00c5");
    internal static readonly Guid MechanicalKeyboardId = Guid.Parse("db6f5fd4-bf4e-44b9-88b9-6df6cfd0a13e");
    internal static readonly Guid ErgonomicChairId = Guid.Parse("bc7f9c55-ad74-448f-870b-13b8b82f501c");
    internal static readonly Guid StandingDeskId = Guid.Parse("7593a8b6-c0ae-4b0b-b350-37deaf436fc2");
    internal static readonly Guid SmartWatchId = Guid.Parse("6ea52fd5-f353-42d0-af5e-e09a7997fa8a");
    internal static readonly Guid YogaMatId = Guid.Parse("2dd712d1-7cee-4b47-987a-446eb1907f6a");

    internal static readonly Guid ReviewOneId = Guid.Parse("99a30d2c-9cfb-42cb-8ea0-edc0d5cb3af8");
    internal static readonly Guid ReviewTwoId = Guid.Parse("eb674a0a-cf6a-4d3a-991c-fc95fbc0f944");
    internal static readonly Guid ReviewThreeId = Guid.Parse("08b76ca9-3af4-4dfd-b6d7-4cd7993eef40");
    internal static readonly Guid ReviewFourId = Guid.Parse("cf013e87-4588-4230-aad2-6ed6920221da");
    internal static readonly Guid ReviewFiveId = Guid.Parse("82eb6ca2-e910-4f2c-ab2d-504c5b71501e");
    internal static readonly Guid ReviewSixId = Guid.Parse("01112e2f-7d32-4cb5-92b4-e0b5244620fe");

    private static readonly DateTime SeedCreatedAt = new(2026, 4, 14, 0, 0, 0, DateTimeKind.Utc);
    private static readonly DateTime SeedUpdatedAt = new(2026, 4, 14, 0, 0, 0, DateTimeKind.Utc);
    private static readonly DateTime SeedRefreshTokenExpiryDate = new(2026, 5, 14, 0, 0, 0, DateTimeKind.Utc);

    internal static IEnumerable<object> Users =>
    [
        new
        {
            Id = ReviewerOneId,
            FullName = "Lena Carter",
            CreatedAt = SeedCreatedAt,
            RefreshToken = (string?)null,
            RefreshTokenExpiryDate = SeedRefreshTokenExpiryDate,
            UserName = "lena.carter",
            NormalizedUserName = "LENA.CARTER",
            Email = "lena.carter@example.com",
            NormalizedEmail = "LENA.CARTER@EXAMPLE.COM",
            EmailConfirmed = true,
            PasswordHash = (string?)null,
            SecurityStamp = "B24E9502-B337-45E0-AF6D-5F1133AE0B0A",
            ConcurrencyStamp = "1E5EB718-66E6-4596-9091-0D76EF394295",
            PhoneNumber = (string?)null,
            PhoneNumberConfirmed = false,
            TwoFactorEnabled = false,
            LockoutEnd = (DateTimeOffset?)null,
            LockoutEnabled = false,
            AccessFailedCount = 0,
            IsVipUser = true,
            Points = 240
        },
        new
        {
            Id = ReviewerTwoId,
            FullName = "Marcus Reed",
            CreatedAt = SeedCreatedAt,
            RefreshToken = (string?)null,
            RefreshTokenExpiryDate = SeedRefreshTokenExpiryDate,
            UserName = "marcus.reed",
            NormalizedUserName = "MARCUS.REED",
            Email = "marcus.reed@example.com",
            NormalizedEmail = "MARCUS.REED@EXAMPLE.COM",
            EmailConfirmed = true,
            PasswordHash = (string?)null,
            SecurityStamp = "42D1AB89-0A11-4A80-A784-0343D5C5DE4B",
            ConcurrencyStamp = "5403C123-50E6-4292-A88F-774830F1B330",
            PhoneNumber = (string?)null,
            PhoneNumberConfirmed = false,
            TwoFactorEnabled = false,
            LockoutEnd = (DateTimeOffset?)null,
            LockoutEnabled = false,
            AccessFailedCount = 0,
            IsVipUser = false,
            Points = 95
        }
    ];

    internal static IEnumerable<object> ProductCategories =>
    [
        new
        {
            Id = ElectronicsCategoryId,
            Name = "Electronics",
            Description = "Connected devices and premium personal tech.",
            IsDeleted = false
        },
        new
        {
            Id = HomeOfficeCategoryId,
            Name = "Home Office",
            Description = "Desk setups and accessories built for all-day comfort.",
            IsDeleted = false
        },
        new
        {
            Id = FitnessCategoryId,
            Name = "Fitness",
            Description = "Workout essentials and recovery-focused gear.",
            IsDeleted = false
        }
    ];

    internal static IEnumerable<object> Products =>
    [
        new
        {
            Id = NoiseCancellingHeadphonesId,
            CategoryId = ElectronicsCategoryId,
            Name = "Auralux Noise Cancelling Headphones",
            Description = "Wireless over-ear headphones with adaptive ANC and 35-hour battery life.",
            QuantityInStock = 42,
            UnitPrice = 249.99m,
            IsDeleted = false,
            CreatedAt = SeedCreatedAt,
            UpdatedAt = SeedUpdatedAt
        },
        new
        {
            Id = MechanicalKeyboardId,
            CategoryId = ElectronicsCategoryId,
            Name = "Pulse Mechanical Keyboard",
            Description = "Low-profile mechanical keyboard with hot-swappable switches and white backlight.",
            QuantityInStock = 68,
            UnitPrice = 129.50m,
            IsDeleted = false,
            CreatedAt = SeedCreatedAt,
            UpdatedAt = SeedUpdatedAt
        },
        new
        {
            Id = ErgonomicChairId,
            CategoryId = HomeOfficeCategoryId,
            Name = "Contour Ergonomic Chair",
            Description = "Mesh ergonomic chair with adjustable lumbar support and 4D armrests.",
            QuantityInStock = 15,
            UnitPrice = 389.00m,
            IsDeleted = false,
            CreatedAt = SeedCreatedAt,
            UpdatedAt = SeedUpdatedAt
        },
        new
        {
            Id = StandingDeskId,
            CategoryId = HomeOfficeCategoryId,
            Name = "Rise Standing Desk",
            Description = "Electric standing desk with programmable height presets and cable tray.",
            QuantityInStock = 12,
            UnitPrice = 599.99m,
            IsDeleted = false,
            CreatedAt = SeedCreatedAt,
            UpdatedAt = SeedUpdatedAt
        },
        new
        {
            Id = SmartWatchId,
            CategoryId = FitnessCategoryId,
            Name = "Stride Pro Smart Watch",
            Description = "Fitness smartwatch with GPS, sleep tracking, and heart-rate alerts.",
            QuantityInStock = 34,
            UnitPrice = 199.00m,
            IsDeleted = false,
            CreatedAt = SeedCreatedAt,
            UpdatedAt = SeedUpdatedAt
        },
        new
        {
            Id = YogaMatId,
            CategoryId = FitnessCategoryId,
            Name = "Balance Grip Yoga Mat",
            Description = "Non-slip extra-thick yoga mat designed for home workouts and studio use.",
            QuantityInStock = 77,
            UnitPrice = 39.95m,
            IsDeleted = false,
            CreatedAt = SeedCreatedAt,
            UpdatedAt = SeedUpdatedAt
        }
    ];

    internal static IEnumerable<object> ProductReviews =>
    [
        new
        {
            Id = ReviewOneId,
            ProductId = NoiseCancellingHeadphonesId,
            UserId = ReviewerOneId,
            Stars = 5,
            Comment = "Excellent noise isolation for commuting and the battery easily lasts a full work week.",
            CreatedAt = SeedCreatedAt,
            UpdatedAt = SeedUpdatedAt,
            IsDeleted = false
        },
        new
        {
            Id = ReviewTwoId,
            ProductId = NoiseCancellingHeadphonesId,
            UserId = ReviewerTwoId,
            Stars = 4,
            Comment = "Comfortable fit and clean sound profile, though the carry case is bulkier than expected.",
            CreatedAt = SeedCreatedAt,
            UpdatedAt = SeedUpdatedAt,
            IsDeleted = false
        },
        new
        {
            Id = ReviewThreeId,
            ProductId = MechanicalKeyboardId,
            UserId = ReviewerOneId,
            Stars = 5,
            Comment = "Tactile without being too loud. It feels premium and works well for both coding and writing.",
            CreatedAt = SeedCreatedAt,
            UpdatedAt = SeedUpdatedAt,
            IsDeleted = false
        },
        new
        {
            Id = ReviewFourId,
            ProductId = ErgonomicChairId,
            UserId = ReviewerTwoId,
            Stars = 4,
            Comment = "Assembly took a bit of time, but the lumbar support made a noticeable difference after a few days.",
            CreatedAt = SeedCreatedAt,
            UpdatedAt = SeedUpdatedAt,
            IsDeleted = false
        },
        new
        {
            Id = ReviewFiveId,
            ProductId = SmartWatchId,
            UserId = ReviewerOneId,
            Stars = 4,
            Comment = "Accurate activity tracking and a bright display. The companion app could be a little faster.",
            CreatedAt = SeedCreatedAt,
            UpdatedAt = SeedUpdatedAt,
            IsDeleted = false
        },
        new
        {
            Id = ReviewSixId,
            ProductId = YogaMatId,
            UserId = ReviewerTwoId,
            Stars = 5,
            Comment = "Great grip on hardwood floors and enough padding for longer mobility sessions.",
            CreatedAt = SeedCreatedAt,
            UpdatedAt = SeedUpdatedAt,
            IsDeleted = false
        }
    ];
}