namespace OrbitWatch.Models.Api
{
    public class CelesTrakResult
    {
        public CelesTrakGpData? Data { get; set; }
        public bool IsStale { get; set; }
    }
}
