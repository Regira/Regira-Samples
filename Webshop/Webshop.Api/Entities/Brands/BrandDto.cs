namespace Webshop.Api.Entities.Brands;

public class BrandCoreDto
{
    public int Id { get; set; }
    public string Title { get; set; } = null!;
}

public class BrandDto : BrandCoreDto
{
    public string? Description { get; set; }
    public string? Country { get; set; }
    public string? Website { get; set; }
    public DateTime Created { get; set; }
    public DateTime? LastModified { get; set; }
}

public class BrandInputDto
{
    public int Id { get; set; }
    public string Title { get; set; } = null!;
    public string? Description { get; set; }
    public string? Country { get; set; }
    public string? Website { get; set; }
}
