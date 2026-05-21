namespace Shared.Domain.Models;

public record WorkersCount
{
    public int CurrentNumberOfWorkers { get; set; }
    public int DesiredNumberOfWorkers { get; set; }
}