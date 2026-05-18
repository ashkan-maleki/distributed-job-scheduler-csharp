using Bogus;
using Master.Domain.Models;
using Master.Domain.Repositories;
using Master.Domain.Services;
using Shared.Domain.Failures;

namespace Master.App.Services;

public class WorkerService(IWorkerRepository workerRepository) : IWorkerService
{
    public List<Worker> Workers => workerRepository.Workers;
    public async Task<IError?> Scale(int count)
    {
        IError? error = null;
        Faker faker = new();
        for (int i = 0; i < count - workerRepository.WorkersCount; i++)
        {
            error =  await workerRepository.AddAsync(new Worker(faker.Company.CompanyName()));
            if (error != null)
            {
                return error;
            }
        }
        while (workerRepository.WorkersCount > count)
        {
            (error, Worker? worker) = await workerRepository.FirstAsync();
            if (error != null)
            {
                return error;
            }
            error = await workerRepository.RemoveAsync(worker!);
            if (error != null)
            {
                return error;
            }
        }
        error = await workerRepository.UnitOfWork.SaveEntitiesAsync();
        if (error != null)
        {
            return error;
        }
        return null;
    }
}