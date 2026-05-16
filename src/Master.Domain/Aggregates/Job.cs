using Master.Domain.Models;
using Shared.Domain.Failures;

namespace Master.Domain.Aggregates;


public record Job(string Name)
{
    public Guid Id { get; init; } = Guid.NewGuid();
    public JobState State { get; init; } = JobState.Queued;
    public Worker? Worker { get; init; }
    public Guid? WorkerId => Worker?.Id;
    public Job Assign(Worker worker)
    {
        return this with { State = JobState.Assigned, Worker = worker};
    }

    public (IError?, Job?) TryStart(Guid workerId)
    {
        if (WorkerId != workerId)
        {
            return new(new JobError("This job is already assigned to another worker."), null);
        }

        if (State != JobState.Assigned)
        {
            return new(new JobError("Job is in wrong state."), null);
        }

        Job job = this with { State = JobState.Running };
        return new(null, job);
    }
    
    public (IError?, Job?) TryComplete(Guid workerId)
    {
        if (WorkerId != workerId)
        {
            return new(new JobError("This job is already assigned to another worker."), null);
        }

        if (State != JobState.Running)
        {
            return new(new JobError("Job is in wrong state."), null);
        }

        Job job = this with { State = JobState.Completed };
        return new(null, job);
    }
    
    public (IError?, Job?) TryFail(Guid workerId)
    {
        if (WorkerId != workerId)
        {
            return new(new JobError("This job is already assigned to another worker."), null);
        }

        if (State != JobState.Running)
        {
            return new(new JobError("Job is in wrong state."), null);
        }

        Job job = this with { State = JobState.Failed };
        return new(null, job);
    }
}

public class JobError(string message) : Error<Job>(message);