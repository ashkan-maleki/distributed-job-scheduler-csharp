namespace Master.Domain.Models;

public class SchedulerState
{
    public int CurrentNumberOfWorkers { get; set; } = 0;
    public int DesiredNumberOfWorkers { get; set; }
}