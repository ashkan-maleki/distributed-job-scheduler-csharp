namespace Master.Domain.Models;

public class WorkersState
{
    private WorkersState()
    {
        
    }

    public WorkersState(int desiredNumberOfWorkers, int currentNumberOfWorkers)
    {
        DesiredNumberOfWorkers = desiredNumberOfWorkers;
        NumberOfWorkersToRegister = desiredNumberOfWorkers - currentNumberOfWorkers;
    }

    public int Id { get; set; }
    
    public int DesiredNumberOfWorkers { get; set; }
    public int NumberOfWorkersToRegister { get; set; }

    public bool RegistrationAllowed => NumberOfWorkersToRegister > 0;


    public void Register()
    {
        NumberOfWorkersToRegister--;
    }
}