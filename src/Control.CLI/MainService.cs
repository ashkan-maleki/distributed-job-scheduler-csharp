namespace Control.CLI;

public class MainService(HttpClient client)
{
    public async Task<bool> CheckMasterAvailability()
    {
        int retryCount = 0;
        while (true)
        {
            try
            {
                if (retryCount >= 10)
                {
                    Console.WriteLine("Master is unavailable.");
                    return false;
                }
                HttpResponseMessage response =
                    await client.GetAsync("/api/hc");

                response.EnsureSuccessStatusCode();

                Console.WriteLine("Master is available.");

                break;
            }
            catch (HttpRequestException e)
            {
                retryCount++;
                Console.WriteLine(
                    $"Health check failed: {e.Message}, attempt: {retryCount}");

                await Task.Delay(5000);
            }
        }
        return true;
    }

    public async Task<bool> Program()
    {
        string? input = Console.ReadLine();
        JobService jobService = new (client);
        WorkerService workerService = new (client);
        SchedulerStateService schedulerStateService = new (client);
        if (input == "q")
        {
            return false;
        }
        if (input == "job new")
        {
            await jobService.New();    
        }
        if (input == "job all")
        {
            await jobService.All();
        }
        if (input == "worker all")
        {
            await workerService.All();
        }
        if (input == "state scale")
        {
            await schedulerStateService.Scale();
        }
        if (input == "state count")
        {
            await schedulerStateService.WorkersCount();
        }
        return true;
    }
}