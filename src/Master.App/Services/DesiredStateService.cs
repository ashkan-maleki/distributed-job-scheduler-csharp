using Bogus;
using Master.Domain.Models;
using Master.Domain.Repositories;
using Master.Domain.Services;
using Shared.Domain.DTOs;

namespace Master.App.Services;

public class DesiredStateService(IDesiredStateRepository desiredStateRepository) : IDesiredStateService
{
    public async Task<IResult> ScaleAsync(int desiredNumberOfWorkers)
    {
        Result<DesiredState> result = await desiredStateRepository.GetAsync();
        if (result.TryGetValue(out DesiredState? schedulerState))
        {
            desiredStateRepository.Remove(schedulerState);
        }
        await desiredStateRepository.AddAsync(new DesiredState(desiredNumberOfWorkers));
        return await desiredStateRepository.UnitOfWork.SaveEntitiesAsync();
    }


    public async Task<int> DesiredNumberOfWorkersAsync()
    {
        Result<DesiredState> result = await desiredStateRepository.GetAsync();
        if (!result.TryGetValue(out DesiredState? schedulerState))
        {
            return 0;
        }

        return schedulerState.DesiredNumberOfWorkers;
    }
    
    
    
}