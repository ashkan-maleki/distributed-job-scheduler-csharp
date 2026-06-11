namespace Master.Domain.Models;

public class DesiredState(int desiredNumberOfWorkers)
{
    public int Id { get; set; }
    public int DesiredNumberOfWorkers { get; set; } = desiredNumberOfWorkers;
}

public record DesiredStateMessage(int DesiredNumberOfWorkers);