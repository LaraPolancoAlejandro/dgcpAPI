namespace dgcp.domain.Models;

public class TenderItem
{
    public Guid Id { get; set; }
    // Rubro
    public int Classification { get; set; }

    public Tender Tender { get; set; }
}
