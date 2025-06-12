using GroqSharp.Models;

namespace GroqSharp.Interfaces
{
    public interface IGroqConfigurationService
    {
        GroqConfiguration GetConfiguration();
    }
}
