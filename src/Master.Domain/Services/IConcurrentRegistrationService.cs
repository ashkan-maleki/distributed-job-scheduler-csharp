using Master.Domain.Models;
using Shared.Domain.DTOs;

namespace Master.Domain.Services;

public interface IConcurrentRegistrationService
{
    public Task<Result<Worker>> RegisterAsync();
}