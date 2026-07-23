namespace ProyectoAvengers.Shared.DTOs.Admin;

public class SiteSettingDto
{
    public string Key { get; set; } = string.Empty;
    public string? Value { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

public class UpdateSiteSettingRequest
{
    public string? Value { get; set; }
}
