using Shared.Domain.DTOs;

namespace Master.Domain.Services;

public interface IDesiredStateService
{
    public Task<IResult> ScaleAsync(int desiredNumberOfWorkers);
    public Task<int> DesiredNumberOfWorkersAsync();
}

