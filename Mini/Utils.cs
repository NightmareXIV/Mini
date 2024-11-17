using ECommons;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Mini;
internal static unsafe class Utils
{
    public static Type? GetTypeFromRuntimeAssembly(string assemblyName, string type)
    {
        try
        {
            var fType = Assembly.Load(assemblyName);
            var t = fType.GetType(type);
            return t;
        }
        catch(Exception e)
        {
            e.Log();
        }
        return null;
    }
}
