namespace Master.Domain.Models;

public class WorkersState(int desiredNumberOfWorkers, int currentNumberOfWorkers)
{
    public int DesiredNumberOfWorkers { get; set; } = desiredNumberOfWorkers;
    public int NumberOfWorkersToRegister { get; set; } = desiredNumberOfWorkers -  currentNumberOfWorkers;
    
    public bool RegistrationAllowed => NumberOfWorkersToRegister > 0;
    

    public void Register()
    {
        NumberOfWorkersToRegister--;
    }
}