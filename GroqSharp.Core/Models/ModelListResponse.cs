namespace GroqSharp.Core.Models
{
    public class ModelListResponse
    {
        public string Object { get; set; }
        public ModelData[] Data { get; set; }
    }

    public class ModelData
    {
        public string Id { get; set; }
        public string Object { get; set; }
        public long Created { get; set; }
        public string Owned_By { get; set; }
    }
}
