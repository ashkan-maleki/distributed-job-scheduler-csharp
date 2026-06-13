using Master.Domain.Models;
using Master.Domain.Repositories;
using Master.Domain.Services;
using Shared.Domain.DTOs;

namespace Master.App.Services;

public class WorkersStateService(IWorkersStateRepository workersStateRepository) : IWorkersStateService
{
    public bool RegistrationAllowed
    {
        get
        {
            Result<WorkersState> result = workersStateRepository.Get();
            if (result.NotFound) 
                return false;
            return result.OkResult.Value.RegistrationAllowed;
        }
    }

    public async Task<IResult> RegisterAsync()
    {
        Result<WorkersState> result = await workersStateRepository.GetAsync();
        if (result.NotFound) 
            return result.NotFoundResult;
        result.OkResult.Value.Register();
        return await workersStateRepository.UnitOfWork.SaveEntitiesAsync();
    }

    public async Task<IResult> AddAsync(WorkersState workersState)
    {
        await workersStateRepository.AddAsync(workersState);
        return await workersStateRepository.UnitOfWork.SaveEntitiesAsync();
    }
}