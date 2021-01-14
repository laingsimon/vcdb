using Microsoft.Extensions.DependencyInjection;
using System;
using System.Linq;

namespace vcdb.DependencyInjection
{
    public class ServiceNamespaceRegistrationBuilder
    {
        private readonly Type namespacedType;
        private readonly IServiceCollection services;

        public ServiceNamespaceRegistrationBuilder(Type namespacedType, IServiceCollection services)
        {
            this.namespacedType = namespacedType;
            this.services = services;
        }

        public IServiceCollection AddAsSingleton()
        {
            return Add((services, interfaceType, implementationType) => services.AddSingleton(interfaceType, implementationType));
        }

        private IServiceCollection Add(Func<IServiceCollection, Type, Type, IServiceCollection> add)
        {
            var ns = namespacedType.Namespace;
            var classesInNamespace = namespacedType.Assembly.GetTypes().Where(t => t.Namespace == ns && !t.IsAbstract && t.IsClass && !t.IsNested);

            return classesInNamespace.Aggregate(services, (services, classInNamespace) =>
            {
                var interfaces = classInNamespace.GetInterfaces();

                return interfaces.Aggregate(services, (services, interfaceType) =>
                {
                    return add(services, interfaceType, classInNamespace);
                });
            });
        }
    }
}
