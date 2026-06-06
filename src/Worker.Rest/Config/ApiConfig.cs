using Microsoft.Extensions.Options;

namespace Worker.Rest.Config;

public class ApiConfigSetup(IConfiguration configuration) : IConfigureOptions<ApiConfig>
{
    private const string SectionName = "Rpc";

    public void Configure(ApiConfig options) => configuration.GetSection(SectionName).Bind(options);
}

public class ApiConfig
{
    public required MasterApisConfig MasterApis { get; set; }
}

public class MasterApisConfig
{
    public required string BaseAddress { get; set; } 

    public required string HealthCheck { get; set; }
    public required MasterWorkerApis WorkerApis { get; set; }
    public required MasterJobApis JobApis { get; set; }
}

public class MasterWorkerApis
{
    public required string All { get; set; }
    public required string Registration { get; set; }

    public required string HeartBeat { get; set; }
}

public class MasterJobApis
{
    public required string Get { get; set; }
    public required string Start { get; set; } 
    public required string Result { get; set; } 
}