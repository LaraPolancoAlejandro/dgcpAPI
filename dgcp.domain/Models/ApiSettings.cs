namespace dgcp.api
{
    public class Empresa
    {
        public string[] Keywords { get; set; }
        public int[] Categories { get; set; }
    }

    public class ApiSettings
    {
        public string Host { get; set; }
        public Dictionary<string, Empresa> Empresas { get; set; }
        public int UpdateTimeout { get; set; }
    }
}
