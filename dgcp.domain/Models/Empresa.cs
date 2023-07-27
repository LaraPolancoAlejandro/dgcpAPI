namespace dgcp.domain.Models
{
    public class Empresa
    {
        public int ID { get; set; }
        public string NombreEmpresa { get; set; }

        // Navegación a las tablas de relación
        public ICollection<EmpresaKeyword> EmpresaKeywords { get; set; }
        public ICollection<EmpresaRubro> EmpresaRubros { get; set; }
    }

}
