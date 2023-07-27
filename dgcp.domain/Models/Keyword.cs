namespace dgcp.domain.Models
{
    public class Keyword
    {
        public int ID { get; set; }
        public string Palabra { get; set; }

        // Navegación a la tabla de relación
        public ICollection<EmpresaKeyword> EmpresaKeywords { get; set; }
    }
}
