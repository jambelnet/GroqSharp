using GroqSharp.Extensions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace GroqSharp.Library
{
    public class GroqSharpBuilder
    {
        public static IServiceCollection CreateDefaultBuilder(IConfiguration config)
        {
            var services = new ServiceCollection();
            services.AddGroqSharpCore(config);
            return services;
        }
    }
}
