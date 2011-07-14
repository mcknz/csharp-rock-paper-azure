using System;
using System.Collections.Generic;
using System.Reflection;
using Compete.Model.Game;
using Compete.Site.Models;
using System.Linq;

namespace Compete.Site.Infrastructure
{
    public class DynamicAssemblyTypeFinder
    {
        readonly List<Assembly> _assemblies = new List<Assembly>();

        public void AddAssembly(AssemblyFile assemblyFile)
        {
            try
            {
                Assembly assembly = Assembly.LoadFile(assemblyFile.Path);
                _assemblies.Add(assembly);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Error Loading Assembly: " + ex.Message);
            }
        }

        public void AddAll(ICollection<AssemblyFile> files)
        {
            foreach (AssemblyFile file in files)
            {
                AddAssembly(file);
            }
        }

        public IEnumerable<T> Create<T>()
        {
            foreach (Type type in EnumerateTypesOf<T>())
            {
                CreationClock<object> _clock =
                   new CreationClock<object>(null, "botfactory", 200);

                yield return (T)_clock.Run(() => (T)Activator.CreateInstance(type));
            }
        }
        
        public T CreateOne<T>()
        {
            Type type = EnumerateTypesOf<T>().FirstOrDefault();
            if (type != null)
                return (T)Activator.CreateInstance(type);
            else
                return default(T);
        }

        public Type FindType(String typeString)
        {
            foreach (Assembly assembly in _assemblies)
            {
                return assembly.GetExportedTypes().Where((t) => String.Equals(t.FullName, typeString, StringComparison.Ordinal)).FirstOrDefault<Type>();
            }
            return default(Type);
        }


        private IEnumerable<Type> EnumerateTypesOf<T>()
        {
            List<Assembly> loaded = new List<Assembly>();
            foreach (Assembly assembly in _assemblies)
            {
                foreach (Type type in assembly.GetExportedTypes())
                {
                    if (typeof(T).IsAssignableFrom(type) && !type.IsAbstract)
                    {
                        loaded.Add(type.Assembly);
                        yield return type;
                    }
                }
            }
        }
    }
}
