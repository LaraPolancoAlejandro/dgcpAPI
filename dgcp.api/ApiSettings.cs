namespace dgcp.api
{
    public class ApiSettings
    {
        public string Host { get; set; }
        public int[] Categories { get; set; }
        public string[] Keywords { get; set; }
        public int UpdateTimeout { get; set; }
    }
}