using Master.App.EF;
using Master.Domain.Aggregates;
using Master.Domain.Repositories;
using Microsoft.EntityFrameworkCore;
using Shared.Domain.DTOs;
using Shared.Domain.EF;

namespace Master.App.Repositories;

public class JobRepository() : IJobRepository
{
    private readonly SchedulerDbContext _context;
    public IUnitOfWork UnitOfWork => _context;
    
    object @lock = new object();
    // ConcurrentDictionary<Guid, Job> jobs = new();

    public JobRepository(SchedulerDbContext context) : this()
    {
        _context = context;
        if (!_context.Jobs.Any())
        {
            Job item = new Job("Job 1: Wash dishes");
            // item.Queue();
            Job item1 = new Job("Job 2: Clean your room");
            // item.Queue();
            Job job1 = new Job("Job 3: Work on the garden");
            // item.Queue();
        
            _context.Jobs.AddRange(item, item1, job1);
            _ = UnitOfWork.SaveEntitiesAsync();
        }
    }


    public async Task<List<Job>> AllAsync() => await _context.Jobs.ToListAsync();

    public async Task<IMessage?> AddAsync(Job job)
    {
        _ = await _context.Jobs.AddAsync(job);
        return null;
    }

    public async Task<(IMessage?, Job?)> DequeueAsync()
    {
        Job? queuedJob = await _context.Jobs.Where(j => j.State == JobState.Queued).FirstOrDefaultAsync();
        if (queuedJob == null)
        {
            return new(new JobRepositoryNotFoundError($"There are no job in queue"), null);
        }
        return (null, queuedJob);
    }

    // public async Task<IError?> Update(Job newJob,  Job oldJob)
    // {
    //     _ = _context.Jobs.Update(newJob);
    //     return null;
    // }

    public async Task<(IMessage?, Job?)> GetAsync(Guid jobId)
    {
        Job? job = await _context.Jobs.Where(j => j.Id == jobId).FirstOrDefaultAsync();
        if (job == null)
        {
            return new(new JobRepositoryNotFoundError($"There are no job in queue with id as {jobId}"), null);
        }
        return (null, job);
    }

    
}