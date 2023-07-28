﻿namespace dgcp.domain.Models;

public class TenderFinal
{
    public Guid Id { get; set; }
    public string ReleaseId { get; set; }
    public string ReleaseOcid { get; set; }
    public string TenderId { get; set; }
    public string Publisher { get; set; }
    public DateTime? PublishedDate { get; set; }
    public string PublicationPolicy { get; set; }
    public string Description { get; set; }
    public DateTime? Date { get; set; }
    public string Status { get; set; }
    public decimal Amount { get; set; }
    public string Currency { get; set; }
    public string ProcuringEntity { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public string DocumentUrl { get; set; }
}
