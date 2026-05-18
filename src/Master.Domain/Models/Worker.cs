using System.ComponentModel.DataAnnotations;

namespace Master.Domain.Models;

public record Worker(string Name, long Version = 1)
{
    public Guid Id { get; init; } = Guid.NewGuid();
    // [Timestamp]
    // public byte[] Version { get; set; }
    public long Version { get; set; } = Version;
}