using Master.App.EF;
using Master.Domain.Aggregates;
using Master.Domain.Repositories;
using Microsoft.EntityFrameworkCore;
using Shared.Domain.DTOs;
using Shared.Domain.EF;

namespace Master.App.Repositories;

public class JobRepository(SchedulerDbContext dbContext) : Repository(dbContext), IJobRepository
{

    // object @lock = new object();

    // public JobRepository(SchedulerDbContext context) : this()
    // {
    //     DbContext = context;
        // if (!DbContext.Jobs.Any())
        // {
        //     Job item = new Job("Job 1: Wash dishes");
        //     // item.Queue();
        //     Job item1 = new Job("Job 2: Clean your room");
        //     // item.Queue();
        //     Job job1 = new Job("Job 3: Work on the garden");
        //     // item.Queue();
        //
        //     DbContext.Jobs.AddRange(item, item1, job1);
        //     _ = UnitOfWork.SaveEntitiesAsync();
        // }
    // }


    public async Task<List<Job>> AllAsync() => await DbContext.Jobs.ToListAsync();
    
    public async Task<Result<Job>> GetAsync(Guid jobId)
    {
        Job? job = await DbContext.Jobs.FindAsync(jobId);
        if (job is null)
        {
            return new NotFound($"There are no job in queue with id as {jobId}");
        }

        return job;
    }
    
    public async Task<Result<Job>> GetQueuedJobAsync()
    {
        Job? job = await DbContext.Jobs.Where(j => j.State == JobState.Queued).FirstOrDefaultAsync();
        if (job is null)
        {
            return new NotFound("There are no job in queue");
        }
        return job;
    }

    public async Task AddAsync(Job job) => _ = await DbContext.Jobs.AddAsync(job);

    // public async Task<IError?> Update(Job newJob,  Job oldJob)
    // {
    //     _ = DbContext.Jobs.Update(newJob);
    //     return null;
    // }
}