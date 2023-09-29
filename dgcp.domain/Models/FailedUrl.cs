namespace dgcp.domain.Models;

public class FailedUrl
{
    public int Id { get; set; }
    public string Url { get; set; }
    public DateTime VisitDate { get; set; }
    public int RetryCount { get; set; }
    public bool IsPermanentlyFailed { get; set; }
}
