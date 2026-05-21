using System.Net.Http.Json;
using Shared.Domain.Models;

namespace Control.CLI;

public class WorkerService(HttpClient client)
{
    public async Task All()
    {
        HttpResponseMessage httpResponseMessage = await client.GetAsync("/api/worker");
        if (httpResponseMessage.StatusCode == System.Net.HttpStatusCode.NoContent)
        {
            Console.WriteLine("No workers found");    
            return;
        }
        
        List<Worker>? workers = await httpResponseMessage.Content.ReadFromJsonAsync<List<Worker>>();
        foreach (Worker worker in workers)
        {
            Console.WriteLine(worker);
        }
    }
    
    public async Task Register()
    {
        HttpResponseMessage httpResponseMessage = await client.GetAsync("/api/worker/register");
        if (httpResponseMessage.StatusCode == System.Net.HttpStatusCode.NoContent)
        {
            Console.WriteLine("No workers found");    
            return;
        }
        
        List<Worker>? workers = await httpResponseMessage.Content.ReadFromJsonAsync<List<Worker>>();
        foreach (Worker worker in workers)
        {
            Console.WriteLine(worker);
        }
    }
}