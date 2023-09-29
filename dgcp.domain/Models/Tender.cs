using System.ComponentModel.DataAnnotations.Schema;

namespace dgcp.domain.Models;

public class Tender
{
    public string ReleaseOcid { get; set; }
    public string ReleaseId { get; set; }
    public string TenderId { get; set; }

    public string Publisher { get; set; }

    public required string Description { get; set; }

    public DateTime? Date { get; set; }

    public string Status { get; set; }
    public decimal Amount { get; set; }
    public string Currency { get; set; }

    public string ProcuringEntity { get; set; }

    public DateTime StartDate { get; set; }
    public DateTime? EndDate { get; set; }

    public List<TenderItem> Items { get; set; } = new List<TenderItem>();

    public string DocumentUrl { get; set; }
    [NotMapped]
    public string? EmpresaIds { get; set; }

}