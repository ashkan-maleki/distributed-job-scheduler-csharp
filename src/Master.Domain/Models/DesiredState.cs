namespace Master.Domain.Models;

public class DesiredState(int desiredNumberOfWorkers, int currentNumberOfWorkers)
{
    public int Id { get; set; }
    public int DesiredNumberOfWorkers { get; set; } = desiredNumberOfWorkers;
    public DateTime StartTime { get; set; } = DateTime.Now;
    public DateTime EndTime { get; set; } = DateTime.MinValue;
    
    private int NumberOfWorkersToRegister { get; set; } = desiredNumberOfWorkers -  currentNumberOfWorkers;
    
    public bool AnyWorkersToRegister => NumberOfWorkersToRegister > 0;
    

    public void Register()
    {
        NumberOfWorkersToRegister--;
        if (!AnyWorkersToRegister)
        {
            EndTime = DateTime.Now;
        }
    }
}

public record DesiredStateMessage(int DesiredNumberOfWorkers);