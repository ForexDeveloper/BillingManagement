using System.Reflection;

namespace Shared.MediatR.Configurations
{
    public class MediatRConfiguration
    {
        private Assembly[] Assemblies { get; set; } = Array.Empty<Assembly>();
        public bool EnableAutoValidation { get; set; } = true;
        public bool EnableAutoLogging { get; set; }
        public bool FullLog { get; set; } = true;
        public Assembly[] GetAssemblies()
        {
            return Assemblies;
        }
        public void SetAssemblies(Assembly[] assemblies)
        {
            Assemblies = assemblies;
        }
    }
}
