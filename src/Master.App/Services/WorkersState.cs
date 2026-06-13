namespace Master.App.Services;

public class WorkersState
{
    public int DesiredNumberOfWorkers { get; set; }
    public int NumberOfWorkersToRegister { get; set; }
    
    public bool AnyWorkersToRegister => NumberOfWorkersToRegister > 0;
    

    public void Register()
    {
        NumberOfWorkersToRegister--;
    }
}