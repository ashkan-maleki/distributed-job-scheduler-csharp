using Shared.Domain.DTOs;

namespace Master.Domain.Aggregates;

public class Job()
{
    public Guid Id { get; init; } = Guid.NewGuid();
    public JobState State { get; private set; } = JobState.Queued;

    public Guid? WorkerId { get; private set; }

    // [Timestamp]
    // public byte[] Version { get; set; }
    public string Name { get; private set; } = string.Empty;
    public long Version { get; private set; } = 1;

    public Job(string name) : this()
    {
        Name = name;
    }

    private IResult ChangeState(JobState oldState, JobState newState, Guid workerId)
    {
        if (WorkerId is not null && WorkerId != workerId)
        {
            return new Error(
                $"Job is already assigned to another worker, {WorkerId}, requesting worker {workerId}.");
        }

        if (State != oldState)
        {
            return new Error(
                $"Job is in wrong state, current state: {State}, expected current state: {oldState}.");
        }

        WorkerId ??= workerId;
        State = newState;
        Version++;
        return new Ok();
    }

    public IResult Assign(Guid workerId) => ChangeState(JobState.Queued, JobState.Assigned, workerId);

    public IResult Start(Guid workerId) => ChangeState(JobState.Assigned, JobState.Running, workerId);

    public IResult Complete(Guid workerId) => ChangeState(JobState.Running, JobState.Completed, workerId);

    public IResult Fail(Guid workerId) => ChangeState(JobState.Running, JobState.Failed, workerId);
}

class JobDeprecated()
{
    public Guid Id { get; init; } = Guid.NewGuid();
    public JobState State { get; private set; } = JobState.Queued;

    public Guid? WorkerId { get; private set; }

    // [Timestamp]
    // public byte[] Version { get; set; }
    public string Name { get; private set; } = string.Empty;
    public long Version { get; private set; }

    public JobDeprecated(string name) : this()
    {
        Name = name;
        // Version = 1;
    }

    private Result2 ChangeState(JobState oldState, JobState newState, Guid? workerId = null)
    {
        if (workerId is not null && WorkerId != workerId)
        {
            return Results.DomainFailure(
                $"Job is already assigned to another worker, {WorkerId}, requesting worker {workerId}.");
        }

        if (State != oldState)
        {
            return Results.DomainFailure(
                $"Job is in wrong state, current state: {oldState}, new state: {newState}.");
        }

        State = newState;
        return Results.Ok();
    }

    public Result2 Assign(Guid workerId)
    {
        WorkerId = workerId;
        return ChangeState(JobState.Queued, JobState.Assigned);
        // if (State != JobState.Queued)
        // {
        //     return Results.DomainErrorRaised("Job is in wrong state.");
        // }
        //
        // State = JobState.Assigned;
        // WorkerId = workerId;
        // Version += 1;
        // return Results.NoContent();
    }

    public Result2 Start(Guid workerId)
    {
        return ChangeState(JobState.Assigned, JobState.Running, workerId);
        // if (WorkerId != workerId)
        // {
        //     return Results.DomainErrorRaised("This job is already assigned to another worker.");
        // }
        //
        // if (State != JobState.Assigned)
        // {
        //     return Results.DomainErrorRaised("Job is in wrong state.");
        // }
        //
        // State = JobState.Running;
        // Version += 1;
        // return Results.NoContent();
    }

    public Result2 Complete(Guid workerId)
    {
        return ChangeState(JobState.Running, JobState.Completed, workerId);
        // if (WorkerId != workerId)
        // {
        //     return Results.DomainErrorRaised("This job is already assigned to another worker.");
        // }
        //
        // if (State != JobState.Running)
        // {
        //     return Results.DomainErrorRaised("Job is in wrong state.");
        // }
        //
        // State = JobState.Completed;
        // Version += 1;
        // return Results.NoContent();
    }

    public Result2 Fail(Guid workerId)
    {
        return ChangeState(JobState.Running, JobState.Failed, workerId);
        // if (WorkerId != workerId)
        // {
        //     
        //     return Results.DomainErrorRaised("This job is already assigned to another worker.");
        // }
        //
        // if (State != JobState.Running)
        // {
        //     return Results.DomainErrorRaised("Job is in wrong state.");
        // }
        //
        // State = JobState.Failed;
        // Version += 1;
        // return Results.NoContent();
    }
}