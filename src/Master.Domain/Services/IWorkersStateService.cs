using Master.Domain.Models;
using Shared.Domain.DTOs;

namespace Master.Domain.Services;

public interface IWorkersStateService
{
    bool RegistrationAllowed { get; }
    Task<IResult> RegisterAsync();
    Task<IResult> AddAsync(WorkersState workersState);
}