using System.Collections.Concurrent;
using DistributedJobScheduler.Shared;

namespace Master.Domain;

public interface IError
{
    string Message { get; }
    IError? InnerError  { get; }
    Type Type  { get; }
}
public class Error<T>(string message, IError? innerError = null) : IError
{
    public string Message { get;  } = message;
    public IError? InnerError  { get;  } = innerError;
    public Type Type { get; init; } = typeof(T);

    public override string ToString()
    {
        List<string> errors = [];

        IError? current = this;

        while (current is not null)
        {
            errors.Add(
                $"[{current.Type.Name}] {current.Message}");

            current = current.InnerError;
        }

        return string.Join(
            " --> ",
            errors);
    }
}

public interface IJobStore
{
    public (IError?, Job?) TryQueueJob(string name);
    public (IError?, Job?) TryAssignJob();
    public (IError?, Job?) TryStartJob(Guid jobId);
    public (IError?, Job?) TryCompleteJob(Guid jobId);
    public (IError?, Job?) TryFailJob(Guid jobId);
}


public class JobStoreError(string message) : Error<IJobStore>(message);

public class JobStore : IJobStore
{
    object @lock = new object();
    ConcurrentDictionary<Guid, Job> jobs = new();
    public JobStore()
    {
        Job item = new Job("Job 1: Wash dishes");
        Job item1 = new Job("Job 2: Clean your room");
        Job job1 = new Job("Job 3: Work on the garden");

        jobs.TryAdd(job1.Id, job1);
        jobs.TryAdd(item.Id, item);
        jobs.TryAdd(item1.Id, item1);
    }

    public (IError?, Job?) TryQueueJob(string name)
    {
        Job job = new Job(name);
        if (!jobs.TryAdd(job.Id, job))
        {
            return new(new JobStoreError($"Job {job.Name} already exists."), null);
        }
        return  new(null, job);
    }
    
    public (IError?, Job?) TryAssignJob()
    {
        lock (@lock)
        {
            Job? queuedJob = jobs.Values.FirstOrDefault(j => j.State == JobState.Queued);
            if (queuedJob == null) 
            {
                return new (new JobStoreError($"There are no job in queue"), null);
            }
            Job assignedJob = queuedJob with { State = JobState.Assigned };
            if (!jobs.TryUpdate(assignedJob.Id, assignedJob, queuedJob))
            {
                return new(new JobStoreError($"This job, {assignedJob.Name}, is already assigned."), null);
            }
            return new(null, assignedJob);
        }
    }
    
    public (IError?, Job?) TryStartJob(Guid jobId)
    {
        if (!jobs.TryGetValue(jobId, out Job job))
        {
            return new(new JobStoreError("You're so mean for trying to hack us"), null);
        }
        if (job.State != JobState.Assigned)
        {
            return new(new JobStoreError("Job is in wrong state."), null);
        }
        
        Job runningJob = job with { State = JobState.Running };
        if (!jobs.TryUpdate(jobId, runningJob, job))
        {
            return new(new JobStoreError($"Job {jobId} already changed."), null);
        }
        return new(null, runningJob);

    }

    public (IError?, Job?) TryCompleteJob(Guid jobId)
    {
        if (!jobs.TryGetValue(jobId, out Job job))
        {
            return new(new JobStoreError($"You're so mean for trying to hack us."), null);
        }
        if (job.State != JobState.Running)
        {
            return new(new JobStoreError($"Job is in wrong state."), null);
        }
        Job completedJob = job with { State = JobState.Completed };
        if (!jobs.TryUpdate(job.Id, completedJob, job))
        {
            return new(new JobStoreError($"Job {jobId} already changed."), null);
        }
        return new(null, completedJob);
    }

    public (IError?, Job?) TryFailJob(Guid jobId)
    {
        if (!jobs.TryGetValue(jobId, out Job job))
        {
            return new(new JobStoreError($"You're so mean for trying to hack us."), null);
        }
        if (job.State != JobState.Running)
        {
            return new(new JobStoreError($"Job is in wrong state."), null);
        }
        Job failedJob = job with { State = JobState.Failed };
        if (!jobs.TryUpdate(job.Id, failedJob, job))
        {
            return new(new JobStoreError($"Job {jobId} already changed."), null);
        }    
        return new(null, failedJob);
    }



    
}