using Microsoft.Extensions.Options;

namespace Worker.Rest.Config;

public class ApiConfigSetup(IConfiguration configuration) : IConfigureOptions<ApiConfig>
{
    private const string SectionName = "Rpc";

    public void Configure(ApiConfig options) =>
        configuration
            .GetSection(SectionName)
            .Bind(options);
}

public class ApiConfig
{
    public required MasterApisConfig MasterApis { get; set; }
}

public class MasterApisConfig
{
    public string BaseAddress { get; set; } = "http://localhost:5031";

    public string HealthCheck { get; set; } = "/api/hc";
    public required MasterWorkerApis WorkerApis { get; set; }
    public required MasterJobApis JobApis { get; set; }
}

public class MasterWorkerApis
{
    public string All { get; set; } = "/api/worker";
    public string Registration { get; set; } = "/api/worker/register";

    public string HeartBeat { get; set; } = "/api/worker/heartbeat?workerId=";
}

public class MasterJobApis
{
    public string Get { get; set; } = "/api/job?workerId=";
}