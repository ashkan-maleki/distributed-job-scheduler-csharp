using MassTransit;
using Master.Domain.Models;
using Master.Domain.Services;

namespace Master.Rest.Consumers;

public class DesiredStateConsumer(IWorkerService workerService) : IConsumer<DesiredStateMessage>
{
    public async Task Consume(ConsumeContext<DesiredStateMessage> context) => await workerService.ScaleAsync(context.Message.DesiredNumberOfWorkers);
}