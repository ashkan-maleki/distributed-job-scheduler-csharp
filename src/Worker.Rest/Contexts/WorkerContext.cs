namespace Worker.Rest.Contexts;

public class WorkerContext
{
    public DateTime MasterHeartbeatTime { get; set; }

    public bool MasterAvailable => (MasterHeartbeatTime != DateTime.MinValue) && (DateTime.Now - MasterHeartbeatTime < TimeSpan.FromSeconds(5));
    public bool MasterUnavailable => !MasterAvailable;
}