// See https://aka.ms/new-console-template for more information

using Control.CLI;

Console.WriteLine("Welcome to Control Plane!");

HttpClient client = new()
{
    BaseAddress = new Uri("http://localhost:5031")
};

MainService mainService = new (client);
if (!await mainService.CheckMasterAvailability())
{
    return;
}

while (await mainService.Program())
{
    
}


public record JobRequest(string Name);
