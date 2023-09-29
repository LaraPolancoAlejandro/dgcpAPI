using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace dgcp.domain.Models
{
    public class CurrentUrl
    {
        [Key]
        public int Id { get; set; }
        public int CurrentIndex { get; set; }
    }
}
