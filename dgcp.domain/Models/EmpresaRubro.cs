namespace dgcp.domain.Models
{
    public class EmpresaRubro
    {
        public int EmpresaID { get; set; }
        public Empresa Empresa { get; set; }

        public int RubroID { get; set; }
        public Rubro Rubro { get; set; }
    }
}
