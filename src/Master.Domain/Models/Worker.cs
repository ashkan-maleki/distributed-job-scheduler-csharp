namespace Master.Domain.Models;

public class Worker(string name, long version = 1)
{
    public Guid Id { get; init; } = Guid.NewGuid();
    // [Timestamp]
    // public byte[] Version { get; set; }
    public string Name { get; } = name;
    public long Version { get; set; } = version;
    public WorkerState CurrentState { get; set; } =  WorkerState.Idle;
    public DateTime LastHeartBeat { get; set; }
    public DateTime RegisteredAt { get; set; }
    
    public bool HasHeartBeat => DateTime.Now - LastHeartBeat  < TimeSpan.FromSeconds(25);
    public bool IsDead => CurrentState == WorkerState.Dead; 
    public bool IsRegistered => CurrentState == WorkerState.Registered;

    public void Register()
    {
        RegisteredAt = DateTime.Now;
        CurrentState = WorkerState.Registered;
        Version++;
    }

    public void Unregister()
    {
        RegisteredAt = DateTime.MinValue;
        CurrentState = WorkerState.Idle;
        Version++;
    }
    
    public void Kill()
    {
        if (!HasHeartBeat)
            CurrentState = WorkerState.Dead;
    }
    
    public void ReportHeartBeat()
    {
        LastHeartBeat = DateTime.Now;
    }
}

public enum WorkerState
{
    Idle,
    Registered,
    Dead,   
}