using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Reflection;

namespace RelhaxModpack.AtlasesCreator
{
    public static class Importers
    {
        private static List<IImageImporter> imageImporters = new List<IImageImporter>();


        public static ReadOnlyCollection<IImageImporter> ImageImporters { get; private set; }

        public static void Load() { /* invokes static constructor */ }

        static Importers()
        {
            ImageImporters = new ReadOnlyCollection<IImageImporter>(imageImporters);

            // find built in exporters
            FindImporters(Assembly.GetExecutingAssembly());
        }

        private static void FindImporters(Assembly assembly)
        {
            string Namespace = MethodBase.GetCurrentMethod().DeclaringType.Namespace;

            foreach (Type type in assembly.GetTypes())
            {
                if (!type.IsAbstract && type.IsClass && type.Namespace != null && type.Namespace.ToString().Equals(Namespace))
                {
                    try
                    {
                        IImageImporter imageImporter = Activator.CreateInstance(type) as IImageImporter;
                        if (imageImporter != null)
                        {
                            imageImporters.Add(imageImporter);
                        }
                    }
                    catch { /* don't care */ }
                }
            }
        }
    }
}