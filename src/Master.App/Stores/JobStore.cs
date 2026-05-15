using System.Collections.Concurrent;
using Master.Domain.Aggregates;
using Master.Domain.Stores;
using Shared.Domain.Failures;

namespace Master.App.Stores;

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
            return new(new JobStoreOperationError($"Job {job.Name} already exists."), null);
        }

        return new(null, job);
    }

    public (IError?, Job?) TryAssignJob(Guid workerId)
    {
        lock (@lock)
        {
            Job? queuedJob = jobs.Values.Where(j => j.State == JobState.Queued).FirstOrDefault();
            if (queuedJob == null)
            {
                return new(new JobStoreNotFoundError($"There are no job in queue"), null);
            }

            Job assignedJob = queuedJob.Assign(workerId);
            if (!jobs.TryUpdate(assignedJob.Id, assignedJob, queuedJob))
            {
                return new(new JobStoreOperationError($"This job, {assignedJob.Name}, is already assigned."), null);
            }

            return new(null, assignedJob);
        }
    }

    public (IError?, Job?) TryStartJob(Guid jobId, Guid workerId)
    {
        if (!jobs.TryGetValue(jobId, out Job? job))
        {
            return new(new JobStoreNotFoundError($"There are no job in queue with id as {jobId}"), null);
        }
        (IError? error, Job? runningJob) = job.TryStart(workerId);
        if (error is not null)
        {
            return new(error, null);
        }
        if (!jobs.TryUpdate(jobId, runningJob!, job))
        {
            return new(new JobStoreOperationError($"Job {jobId} already changed."), null);
        }

        return new(null, runningJob);
    }

    public (IError?, Job?) TryCompleteJob(Guid jobId, Guid workerId)
    {
        if (!jobs.TryGetValue(jobId, out Job? job))
        {
            return new(new JobStoreNotFoundError($"There are no job in queue with id as {jobId}"), null);
        }

        (IError? error, Job? completedJob) = job.TryComplete(workerId);
        if (error is not null)
        {
            return new(error, null);
        }
        if (!jobs.TryUpdate(job.Id, completedJob!, job))
        {
            return new(new JobStoreOperationError($"Job {jobId} already changed."), null);
        }

        return new(null, completedJob);
    }

    public (IError?, Job?) TryFailJob(Guid jobId, Guid workerId)
    {
        if (!jobs.TryGetValue(jobId, out Job? job))
        {
            return new(new JobStoreNotFoundError($"There are no job in queue with id as {jobId}"), null);
        }

        (IError? error, Job? failedJob) = job.TryFail(workerId);
        if (error is not null)
        {
            return new(error, null);
        }
        
        if (!jobs.TryUpdate(job.Id, failedJob!, job))
        {
            return new(new JobStoreOperationError($"Job {jobId} already changed."), null);
        }

        return new(null, failedJob);
    }
}