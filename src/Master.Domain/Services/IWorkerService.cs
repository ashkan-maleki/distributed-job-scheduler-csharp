using Master.Domain.Models;
using Shared.Domain.Failures;

namespace Master.Domain.Services;

public interface IWorkerService
{
    public Task<List<Worker>> AllAsync();
    public Task<IError?> ScaleAsync(int count);
}