using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options) { }

    public required DbSet<ProcessedEmail> ProcessedEmails { get; set; }
}

public class ProcessedEmail
{
    [Key]
    public string Id { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; }
    public string Subject { get; set; } = string.Empty;
    public string Body { get; set; } = string.Empty;
    public string Response { get; set; } = string.Empty;
    public ProcessingStatus Status { get; set; }
}

public enum ProcessingStatus
{
    InProgress,
    Done,
    Error,
};
