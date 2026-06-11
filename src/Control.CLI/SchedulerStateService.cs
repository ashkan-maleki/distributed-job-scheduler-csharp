using System.Net.Http.Json;
using Shared.Domain.Models;

namespace Control.CLI;

public record ScaleWorkersRequest(int DesiredNumberOfWorkers);
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
        int desiredNumberOfWorkers;
        do
        {
            Console.WriteLine("How many workers do you want to do, my master?");
        } while (!int.TryParse(Console.ReadLine(), out desiredNumberOfWorkers));
        
        ScaleWorkersRequest request = new(desiredNumberOfWorkers);
        HttpResponseMessage response = await client.PostAsJsonAsync("/api/scheduler-states/scale", request);
        response.EnsureSuccessStatusCode();
        Console.WriteLine("Workers scaled successfully");
    }
}