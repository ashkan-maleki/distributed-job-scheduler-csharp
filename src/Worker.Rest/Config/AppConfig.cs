namespace Worker.Rest.Config;

public record AppConfig
{
    public Guid RegistrationToken { get; set; } = Guid.Parse("649c0406-193d-47a5-ae80-e77539c104df");
    public string MasterIpAddress { get; set; } = "http://localhost:5031";
    public string RegistrationApi { get; set; } = "/api/worker/register";
    public string AllWorkersApi { get; set; } = "/api/worker";
    public string MasterHealthCheck { get; set; } = "/api/hc";
}