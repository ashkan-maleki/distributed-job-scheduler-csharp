using System.Net.Http.Json;
using Shared.Domain.Models;

namespace Control.CLI;

public class SchedulerStateService(HttpClient client)
{
    public async Task WorkersCount()
    {
        HttpResponseMessage httpResponseMessage = await client.GetAsync("/api/scheduler-states/workers-count");
        httpResponseMessage.EnsureSuccessStatusCode();
        
        WorkersCount? workersCount = await httpResponseMessage.Content.ReadFromJsonAsync<WorkersCount>();
        Console.WriteLine(workersCount);
    }
    
    public async Task Scale()
    {
        ScaleWorkersRequest request = new(3);
        HttpResponseMessage response = await client.PostAsJsonAsync("/api/scheduler-states/scale", request);
        response.EnsureSuccessStatusCode();
        Console.WriteLine("Workers scaled successfully");
    }
}