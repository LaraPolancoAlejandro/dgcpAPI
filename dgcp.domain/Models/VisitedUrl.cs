using System.ComponentModel.DataAnnotations;

namespace dgcp.domain.Models;

public class VisitedUrl
{
    [Key]
    public string Url { get; set; }
    public DateTime VisitDate { get; set; }
}
