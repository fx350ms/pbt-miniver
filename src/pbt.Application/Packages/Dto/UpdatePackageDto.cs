namespace pbt.Packages.Dto;

public class UpdatePackageDto
{
    public int Id { get; set; }
    public int? BagId { get; set; }
    public bool? IsQuickBagging { get; set; }
}

public class PackageRemoveQuickBaggingDto
{
    public int Id { get; set; }
    public int? BagId { get; set; }
    public bool? IsQuickBagging { get; set; }
}