namespace backend.Services.Model.SeedData;

public class SeedDataOptions
{
    public const string SectionName = "SeedData";

    public List<SeedUserOptions> Users { get; set; } = [];
}
