using Master.Domain.Aggregates;
using Master.Domain.Models;
using Master.Domain.Services;
using Master.Domain.Stores;
using Shared.Domain.Failures;

namespace Master.App.Services;

public class JobService(IJobStore jobStore, IWorkerStore workerStore) : IJobService
{
    public List<Job> Jobs => jobStore.Jobs;

    public (IError?, Job?) TryQueueJob(string name)
    {
        Job job = new Job(name);
        IError? error = jobStore.TryAddJob(job);
        if (error is not null)
        {
            return new(error, null);
        }
        return new(null, job);
    }

    public (IError?, Job?) TryAssignJob(Guid workerId)
    {
        (IError? error, Job? queuedJob) = jobStore.TryDequeueJob();
        if (error is not null)
        {
            return new(error, null);
        }

        (error, Worker? worker) = workerStore.TryGetWorker(workerId);
        if (error is not null)
        {
            return new(error, null);
        }
        Job assignedJob = queuedJob!.Assign(worker!);
        error = jobStore.TryUpdateJob(assignedJob, queuedJob);
        if (error is not null)
        {
            return new(error, null);
        }
        return new(null, assignedJob);
    }

    public (IError?, Job?) TryStartJob(Guid jobId, Guid workerId)
    {
        IError? err;
        (err, Job? job) = jobStore.TryGetJob(jobId);
        if (err is not null)
        {
            return new(err, null);
        }
        (err, Job? runningJob) = job.TryStart(workerId);
        if (err is not null)
        {
            return new(err, null);
        }
        err = jobStore.TryUpdateJob(runningJob!, job);
        if (err is not null)
        {
            return new(err, null);
        }
        return new(null, runningJob);
    }

    public (IError?, Job?) TryCompleteJob(Guid jobId, Guid workerId)
    {
        IError? err;
        (err, Job? job) = jobStore.TryGetJob(jobId);
        if (err is not null)
        {
            return new(err, null);
        }

        (err, Job? completedJob) = job.TryComplete(workerId);
        if (err is not null)
        {
            return new(err, null);
        }
        err = jobStore.TryUpdateJob(completedJob!, job);
        if (err is not null)
        {
            return new(err, null);
        }
        return new(null, completedJob);
    }

    public (IError?, Job?) TryFailJob(Guid jobId, Guid workerId)
    {
        IError? err;
        (err, Job? job) = jobStore.TryGetJob(jobId);
        if (err is not null)
        {
            return new(err, null);
        }

        (err, Job? failedJob) = job.TryFail(workerId);
        if (err is not null)
        {
            return new(err, null);
        }
        err = jobStore.TryUpdateJob(failedJob!, job);
        if (err is not null)
        {
            return new(err, null);
        }

        return new(null, failedJob);
    }
}