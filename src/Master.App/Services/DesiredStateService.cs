using Bogus;
using MassTransit;
using Master.Domain.Models;
using Master.Domain.Repositories;
using Master.Domain.Services;
using Shared.Domain.DTOs;

namespace Master.App.Services;

public class DesiredStateService(IDesiredStateRepository desiredStateRepository, IWorkerRepository workerRepository,
    IPublishEndpoint publishEndpoint) : IDesiredStateService
{
    public async Task<IResult> ScaleAsync(int desiredNumberOfWorkers)
    {
        Result<DesiredState> desiredStateResult = await desiredStateRepository.GetAsync();
        if (desiredStateResult.TryGetValue(out DesiredState? schedulerState))
        {
            desiredStateRepository.Remove(schedulerState);
        }
        int currentNumberOfWorkers = await workerRepository.CountAsync();
        await desiredStateRepository.AddAsync(new DesiredState(desiredNumberOfWorkers, currentNumberOfWorkers));
        IResult result = await desiredStateRepository.UnitOfWork.SaveEntitiesAsync();
        if (result is CriticalError criticalError)
        {
            return criticalError;
        }
        await publishEndpoint.Publish(new DesiredStateMessage(desiredNumberOfWorkers));
        return new Ok();
        
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