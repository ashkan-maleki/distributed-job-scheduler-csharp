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
    
    public List<Job> Jobs => jobs.Values.ToList();

    public IError? TryAddJob(Job job)
    {
        if (!jobs.TryAdd(job.Id, job))
        {
            return new JobStoreOperationError($"Job {job.Name} already exists.");
        }
        return null;
    }

    public (IError?, Job?) TryDequeueJob()
    {
        Job? queuedJob = jobs.Values.Where(j => j.State == JobState.Queued).FirstOrDefault();
        if (queuedJob == null)
        {
            return new(new JobStoreNotFoundError($"There are no job in queue"), null);
        }
        return (null, queuedJob);
    }

    public IError? TryUpdateJob(Job newJob,  Job oldJob)
    {
        if (!jobs.TryUpdate(newJob.Id, newJob, oldJob))
        {
            return new JobStoreOperationError($"This job, {newJob.Name}, is already assigned.");
        }
        return null;
    }

    public (IError?, Job?) TryGetJob(Guid jobId)
    {
        if (!jobs.TryGetValue(jobId, out Job? job))
        {
            return new(new JobStoreNotFoundError($"There are no job in queue with id as {jobId}"), null);
        }
        return (null, job);
    }

    
}