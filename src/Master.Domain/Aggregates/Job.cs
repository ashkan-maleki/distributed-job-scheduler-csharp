using System.ComponentModel.DataAnnotations;
using Shared.Domain.Failures;

namespace Master.Domain.Aggregates;


public class Job()
{
    public Guid Id { get; init; } = Guid.NewGuid();
    public JobState State { get; private set; } = JobState.Queued;
    public Guid? WorkerId { get; private set; }
    // [Timestamp]
    // public byte[] Version { get; set; }
    public string Name { get; private set; } 
    public long Version { get; private  set; }
    
    public Job(string name) : this()
    {
        Name = name;
        Version = 1;
        State = JobState.Queued;
    }

    public IError? Assign(Guid workerId)
    {
        if (State != JobState.Queued)
        {
            return new JobError("Job is in wrong state.");
        }

        State = JobState.Assigned;
        WorkerId =  workerId;
        Version += 1;
        return null;
    }

    public IError? Start(Guid workerId)
    {
        if (WorkerId != workerId)
        {
            return new JobError("This job is already assigned to another worker.");
        }

        if (State != JobState.Assigned)
        {
            return new JobError("Job is in wrong state.");
        }

        State = JobState.Running;
        Version += 1;
        return null;
    }
    
    public IError? Complete(Guid workerId)
    {
        if (WorkerId != workerId)
        {
            return new JobError("This job is already assigned to another worker.");
        }

        if (State != JobState.Running)
        {
            return new JobError("Job is in wrong state.");
        }

        State = JobState.Completed;
        Version += 1;
        return null;
    }
    
    public IError? Fail(Guid workerId)
    {
        if (WorkerId != workerId)
        {
            return new JobError("This job is already assigned to another worker.");
        }

        if (State != JobState.Running)
        {
            return new JobError("Job is in wrong state.");
        }

        State = JobState.Failed;
        Version += 1;
        return null;
    }
}

public class JobError(string message) : Error<Job>(message);