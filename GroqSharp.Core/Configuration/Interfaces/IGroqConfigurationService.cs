using GroqSharp.Core.Configuration.Models;

namespace GroqSharp.Core.Configuration.Interfaces
{
    public interface IGroqConfigurationService
    {
        GroqConfiguration GetConfiguration();
    }
}
