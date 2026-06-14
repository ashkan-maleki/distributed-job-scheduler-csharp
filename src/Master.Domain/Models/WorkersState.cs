using Shared.Domain.DTOs;

namespace Master.Domain.Models;

public class WorkersState
{
    public void Update(int desiredNumberOfWorkers, int currentNumberOfWorkers)
    {
        DesiredNumberOfWorkers = desiredNumberOfWorkers;
        NumberOfWorkersToRegister = desiredNumberOfWorkers - currentNumberOfWorkers;
    }

    
    public int Id { get; set; }
    
    public int DesiredNumberOfWorkers { get; set; }
    public int NumberOfWorkersToRegister { get; set; }

    public bool RegistrationAllowed => NumberOfWorkersToRegister > 0;
    public bool RegistrationNotAllowed => !RegistrationAllowed;

    
    
    public IResult Register()
    {
        if (NumberOfWorkersToRegister <= 0)
        {
            return new DomainFailure("No registration slots available.");
        }

        NumberOfWorkersToRegister--;

        return new Ok();
    }
}