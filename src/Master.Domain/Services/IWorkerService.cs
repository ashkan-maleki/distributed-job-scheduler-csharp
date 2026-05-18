using Master.Domain.Models;
using Shared.Domain.Failures;

namespace Master.Domain.Services;

public interface IWorkerService
{
    public List<Worker> Workers { get; }
    public Task<IError?> Scale(int count);
}