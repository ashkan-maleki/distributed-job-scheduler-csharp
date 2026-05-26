namespace Worker.Rest.Domain;

public class Zarf(Guid WorkerId, Worker Worker,
    CancellationTokenSource CancellationTokenSource,
    Task Task
    );