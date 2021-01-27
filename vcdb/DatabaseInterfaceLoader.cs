using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using vcdb.DependencyInjection;

namespace vcdb
{
    public class DatabaseInterfaceLoader
    {
        private readonly string[] searchDirectories;

        public DatabaseInterfaceLoader(IEnumerable<string> searchDirectories)
        {
            this.searchDirectories = searchDirectories.ToArray();
        }

        public IServicesInstaller GetServicesInstaller(DatabaseType databaseType)
        {
            var assemblyFile = $"vcdb.{databaseType}.dll";

            var assemblyPath = searchDirectories
                .Select(directory => Path.Combine(directory, assemblyFile))
                .Where(assemblyPath => File.Exists(assemblyPath))
                .FirstOrDefault();

            if (assemblyPath == null)
                throw new FileNotFoundException($"Could not find module for specified database type, searched directories: {string.Join("\n", searchDirectories)}", assemblyFile);

            var assembly = Assembly.LoadFrom(assemblyPath);
            var installerTypes = assembly.GetTypes()
                .Where(t => typeof(IServicesInstaller).IsAssignableFrom(t))
                .ToArray();

            if (installerTypes.Length == 0)
                throw new NotSupportedException($"No {nameof(IServicesInstaller)} implementation could be found in interface module: {assemblyFile}");

            var installers = installerTypes
                .Select(CreateInstaller)
                .ToArray();

            return new MultiServicesInstaller(installers);
        }

        private IServicesInstaller CreateInstaller(Type installerType)
        {
            if (installerType.GetConstructor(new Type[0]) == null)
                throw new NotSupportedException($"{installerType.FullName} must have a default constructor");

            return (IServicesInstaller)Activator.CreateInstance(installerType);
        }

        private class MultiServicesInstaller : IServicesInstaller
        {
            private readonly IReadOnlyCollection<IServicesInstaller> installers;

            public MultiServicesInstaller(IReadOnlyCollection<IServicesInstaller> installers)
            {
                this.installers = installers;
            }

            public void RegisterServices(IServiceCollection services)
            {
                foreach (var installer in installers)
                {
                    installer.RegisterServices(services);
                }
            }
        }
    }
}
