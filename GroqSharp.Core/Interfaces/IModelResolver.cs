using GroqSharp.Core.Enums;

namespace GroqSharp.Core.Interfaces
{
    public interface IModelResolver
    {
        string GetModelFor(GroqFeature feature);
        public string GetModel();
        public void SetModel(string newModel);
    }
}
