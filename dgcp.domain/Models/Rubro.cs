namespace dgcp.domain.Models
{
    public class Rubro
    {
        public int ID { get; set; }
        public string CodigoRubro { get; set; }

        // Navegación a la tabla de relación
        public ICollection<EmpresaRubro> EmpresaRubros { get; set; }
    }
}
