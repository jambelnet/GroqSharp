using GroqSharp.Core.Enums;
using GroqSharp.Core.Interfaces;

namespace GroqSharp.Core.Helpers
{
    public static class ModelSelector
    {
        public static string Resolve(IModelResolver resolver, GroqFeature feature, string? overrideModel = null)
            => overrideModel ?? resolver.GetModelFor(feature);
    }
}
