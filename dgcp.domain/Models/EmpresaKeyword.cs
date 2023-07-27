namespace dgcp.domain.Models
{
    // Tablas de relación para las relaciones muchos a muchos
    public class EmpresaKeyword
    {
        public int EmpresaID { get; set; }
        public Empresa Empresa { get; set; }

        public int KeywordID { get; set; }
        public Keyword Keyword { get; set; }
    }
}