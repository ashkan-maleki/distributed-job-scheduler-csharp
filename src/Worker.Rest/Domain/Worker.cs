namespace Worker.Rest.Domain;

public class Worker
{
    public Guid Id { get; init; }
    public string Name { get; init; } = "";
    public CancellationTokenSource CancellationTokenSource { get; init; }
    public Task Task { get; init; } = Task.CompletedTask;
    public DateTime StartedAt { get; init; }
}