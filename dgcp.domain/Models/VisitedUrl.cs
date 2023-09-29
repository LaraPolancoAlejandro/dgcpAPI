using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace dgcp.domain.Models;

public class VisitedUrl
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int ID { get; set; }
    [StringLength(2000)]
    public string Url { get; set; }
    public DateTime VisitDate { get; set; }

}
