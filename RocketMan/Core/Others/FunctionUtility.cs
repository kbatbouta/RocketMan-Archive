using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Verse;

namespace RocketMan
{
    public static class FunctionUtility
    {
        public static Type[] GetLoadableTypes(this Assembly a)
        {
            ICollection<Type> types = new List<Type>();
            try
            {
                types = a.GetTypes().Where(i => i != null).ToList();
            }
            catch (ReflectionTypeLoadException e)
            {
                Log.Warning($"<color=blue>[ROCKETMAN]</color>:[{a.FullName}] Gettypes fallback mod activated!");
                // This is the last line of defense
                // if this fails everything will
                foreach (Type type in e.Types)
                {
                    try
                    {
                        if (type != null && type.Assembly == a)
                            types.Add(type);
                    }
                    // This exception list is not exhaustive, modify to suit any reasons
                    // you find for failure to parse a single assembly
                    catch (BadImageFormatException badImageFormatException)
                    {
                        // Type not in this assembly - reference to elsewhere ignored
                        Log.Error($"ROCKETMAN:[{a.FullName}] {a.FullName} is a bad file! (corrupted):{badImageFormatException}");
                    }
                }
            }
            return types?.ToArray() ?? null;
        }

        public static IEnumerable<Action> GetActions<T>() where T : Attribute
        {
            foreach (var method in Finder.RocketManAssemblies
                .Where(ass => !ass.FullName.Contains("System") && !ass.FullName.Contains("VideoTool"))
                .SelectMany(a => a.GetLoadableTypes())
                .SelectMany(t => t.GetMethods())
                .Where(m => m.HasAttribute<T>())
                .ToArray())
            {
                if (Prefs.DevMode) Log.Message(string.Format("ROCKETMAN: Found action with attribute {0}, {1}:{2}", typeof(T).Name,
                     method.DeclaringType.Name, method.Name));
                yield return () => { method.Invoke(null, null); };
            }
        }

        public static IEnumerable<Func<P>> GetFunctions<T, P>() where T : Attribute
        {
            foreach (var method in Finder.RocketManAssemblies
                .Where(ass => !ass.FullName.Contains("System") && !ass.FullName.Contains("VideoTool"))
                .SelectMany(a => a.GetLoadableTypes())
                .SelectMany(t => t.GetMethods())
                .Where(m => m.HasAttribute<T>())
                .ToArray())
            {
                if (Prefs.DevMode) Log.Message(string.Format("ROCKETMAN: Found function with attribute {0}, {1}:{2}", typeof(T).Name,
                    method.DeclaringType.Name, method.Name));
                yield return () => { return (P)method.Invoke(null, null); };
            }
        }

        public static IEnumerable<Func<P, K>> GetFunctions<T, P, K>() where T : Attribute
        {
            foreach (var method in Finder.RocketManAssemblies
                .Where(ass => !ass.FullName.Contains("System") && !ass.FullName.Contains("VideoTool"))
                .SelectMany(a => a.GetLoadableTypes())
                .SelectMany(t => t.GetMethods())
                .Where(m => m.HasAttribute<T>())
                .ToArray())
            {
                if (Prefs.DevMode) Log.Message(string.Format("ROCKETMAN: Found function with attribute {0}, {1}:{2}", typeof(T).Name,
                    method.DeclaringType.Name, method.Name));
                yield return (input) => (K)method.Invoke(null, new object[] { input });
            }
        }

        public static IEnumerable<Func<P, K, U>> GetFunctions<T, P, K, U>() where T : Attribute
        {
            foreach (var method in Finder.RocketManAssemblies
                .Where(ass => !ass.FullName.Contains("System") && !ass.FullName.Contains("VideoTool"))
                .SelectMany(a => a.GetLoadableTypes())
                .SelectMany(t => t.GetMethods())
                .Where(m => m.HasAttribute<T>())
                .ToArray())
            {
                if (Prefs.DevMode) Log.Message(string.Format("ROCKETMAN: Found function with attribute {0}, {1}:{2}", typeof(T).Name,
                    method.DeclaringType.Name, method.Name));
                yield return (input1, input2) => (U)method.Invoke(null, new object[] { input1, input2 });
            }
        }
    }
}
