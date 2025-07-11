using GroqSharp.Core.Configuration.Models;
using GroqSharp.Core.Enums;
using GroqSharp.Core.Models;

namespace GroqSharp.Core.Configuration.Interfaces
{
    public interface IGroqConfigurationService
    {
        GroqConfiguration GetConfiguration();
        RequestDefaults GetDefaultsFor(GroqFeature feature);
    }
}
