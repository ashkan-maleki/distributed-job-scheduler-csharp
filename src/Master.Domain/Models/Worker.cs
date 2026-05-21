namespace Master.Domain.Models;

public class Worker(string name, long version = 1)
{
    public Guid Id { get; init; } = Guid.NewGuid();
    // [Timestamp]
    // public byte[] Version { get; set; }
    public string Name { get; } = name;
    public long Version { get; set; } = version;
    public WorkerState CurrentState { get; set; } =  WorkerState.Dead;
    public DateTime HeartBeat { get; set; }
    public bool HasHeartBeat => DateTime.Now - HeartBeat  < TimeSpan.FromSeconds(25);
    public bool IsDead => CurrentState == WorkerState.Dead; 
    public bool IsRegistered => CurrentState == WorkerState.Registered;

    public void Register()
    {
        CurrentState = WorkerState.Registered;
    }
    
    public void Kill()
    {
        if (!HasHeartBeat)
            CurrentState = WorkerState.Dead;
    }
    
    public void ReportHeartBeat()
    {
        HeartBeat = DateTime.Now;
    }
}

public enum WorkerState
{
    Dead,
    Registered,
    
}