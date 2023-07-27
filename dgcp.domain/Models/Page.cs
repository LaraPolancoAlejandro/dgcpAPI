namespace dgcp.domain.Models;

public class Paged<T>
{
    public Paged(int? page, int? limit)
    {
        this.Page = page.HasValue && page.Value > 0 ? page.Value : 1;
        this.Limit = limit.HasValue && limit.Value > 0 ? limit.Value : 5;
    }

    public int Page { get; set; }
    public int Limit { get; set; }

    public int Skip => (this.Page - 1) * this.Limit;

    public List<T> Items { get; set; }

    public string Next
    {
        get
        {
            if (this.Items == null || this.Items.Count < Limit)
            {
                return null;
            }
            return $"api/v1/tenders?page={this.Page++}&limit={this.Limit}";
        }
    }
}