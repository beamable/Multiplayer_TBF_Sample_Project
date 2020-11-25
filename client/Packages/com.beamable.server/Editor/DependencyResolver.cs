using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
using System.Text;
using Beamable.Common;
using Beamable.Common.Content;
using Beamable.Server.Editor;
using Beamable.Platform.SDK;
using Beamable.Serialization;
using Beamable.Serialization.SmallerJSON;
using Beamable.Content;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace Beamable.Server
{

    public class DependencyInfo
    {
        public Type Type;
        public AgnosticAttribute Agnostic;
    }

    public class DependencyResolver : MonoBehaviour
    {

        public static HashSet<Type> GetReferencedTypes(Type type)
        {
            var results = new HashSet<Type>();
            if (type == null) return results;

            void Add(Type t)
            {
                results.Add(t);
                if (t.IsGenericType)
                {
                    foreach (var g in t.GenericTypeArguments)
                    {
                        results.Add(g);
                    }
                }
            }

            // get all methods
            Add(type.BaseType);

            var agnosticAttribute = type.GetCustomAttribute<AgnosticAttribute>();
            if (agnosticAttribute != null && agnosticAttribute.SupportTypes != null)
            {
                foreach (var supportType in agnosticAttribute.SupportTypes)
                {
                    Add(supportType);
                }
            }

            foreach (var method in type.GetMethods())
            {
                // TODO: look at the method body itself for type references... https://github.com/jbevain/mono.reflection/blob/master/Mono.Reflection/MethodBodyReader.cs

                Add(method.ReturnType);

                foreach (var parameter in method.GetParameters())
                {
                    Add(parameter.ParameterType);
                }
            }

            // get all fields
            foreach (var field in type.GetFields())
            {
                Add(field.FieldType);
            }

            // get all properties
            foreach (var property in type.GetProperties())
            {
                Add(property.PropertyType);
            }

            // TODO get all generic types

            return new HashSet<Type>(results.Where(t => t != null));
        }

        private static bool IsUnityEngineType(Type t)
        {
            var ns = t.Namespace ?? "";
            var isUnity = ns.StartsWith("UnityEngine") || ns.StartsWith("UnityEditor");
            return isUnity;
        }

        private static bool IsSystemType(Type t)
        {
            var ns = t.Namespace ?? "";
            return ns.StartsWith("System");
        }

        private static bool IsBeamableType(Type t)
        {
            var ns = t.Namespace ?? "";
            if (typeof(Microservice).IsAssignableFrom(t))
            {
                return false; // TODO: XXX hacky and gross, but we *DO* want the Microservice type to get considered...
            }

            /*
             * The namespacing trick won't work for types we haven't converted into a the common namespace yet
             */
            var special = new Type[]
            {
                typeof(PromiseBase),
                //typeof(ContentRef)
            };
            if (special.Any(s => s.IsAssignableFrom(t)))
            {
                return true;
            }


            return ns.StartsWith("Beamable.Common") || ns.StartsWith("Beamable.Server");
        }

        private static bool IsStubbedType(Type t)
        {
            var stubbed = new Type[]
            {
                //typeof(JsonSerializable.ISerializable),
                typeof(ArrayDict),
                typeof(JsonSerializable.IStreamSerializer),
                typeof(ContentObject),
                typeof(IContentRef),

                typeof(ContentDelegate)
            };

            // we stub out any generic references to ContentRef and ContentLink, because those themselves are stubbed.

            if (t.IsGenericType)
            {
                var gt = t.GetGenericTypeDefinition();
                if (typeof(ContentRef<>).IsAssignableFrom(gt) || typeof(ContentLink<>).IsAssignableFrom(gt))
                {
                    return true;
                }
            }



//            var special = new Type[]
//            {
//                typeof(PromiseBase),
//                typeof(ContentRef)
//            };
//            if (special.Any(s => s.IsAssignableFrom(t)))
//            {
//                return true;
//            }

            return stubbed.Any(s => s == t);
        }

        private static bool IsSourceCodeType(Type t, out AgnosticAttribute attribute)
        {
            attribute = t.GetCustomAttribute<AgnosticAttribute>(false);
            return attribute != null;
        }

        public static bool IsMicroserviceRoot(Type t)
        {
            return typeof(Microservice).IsAssignableFrom(t);
        }

        public static string GetTypeName(Type t)
        {
            return t.FullName ?? (t.Namespace + "." + t.Name);
        }

        public static List<DependencyInfo> GetDependencies(MicroserviceDescriptor descriptor)
        {

            Queue<Type> toExpand = new Queue<Type>();
            HashSet<string> seen = new HashSet<string>();
            Dictionary<string, string> trace = new Dictionary<string, string>();

            List<DependencyInfo> infos = new List<DependencyInfo>();

            toExpand.Enqueue(descriptor.Type);
            seen.Add(descriptor.Type.FullName);
            while (toExpand.Count > 0)
            {
                var curr = toExpand.Dequeue();
                var currName = GetTypeName(curr);
                seen.Add(currName);

                // run any sort of white list?

                // filter the types that are unityEngine specific...
                if (IsUnityEngineType(curr))
                {
                    // TODO: Need to further white-list this, because not all Unity types will be stubbed on server.
                    //Debug.Log($"Found Unity Type {currName}");
                    //PrintTrace(currName);
                    continue; // don't go nuts chasing unity types..
                }

                if (IsSystemType(curr))
                {
                    //Debug.Log($"Found System Type {currName}");
                    continue; // don't go nuts chasing system types..
                }

                if (IsBeamableType(curr))
                {
                    continue;
                }

                if (IsStubbedType(curr))
                {
                    //Debug.Log($"Found STUB TYPE {currName}");
                    continue;
                }

                if (IsSourceCodeType(curr, out var agnosticAttribute))
                {
                    // This is good, we can copy this code
                    Debug.Log($"Need to Copy Code {currName} from {agnosticAttribute.SourcePath}");
                    infos.Add(new DependencyInfo
                    {
                        Type = curr,
                        Agnostic = agnosticAttribute
                    });
                    //continue;
                }
                else if (!IsMicroserviceRoot(curr))
                {
                    // no good.
                    Debug.LogError($"Unknown type referenced. Not allowed. {currName}");

                }


                var references = GetReferencedTypes(curr);
                foreach (var reference in references)
                {
                    var referenceName = GetTypeName(reference);
                    if (reference == null || seen.Contains(referenceName))
                    {
                        continue; // we've already seen this type, so march on
                    }

                    seen.Add(referenceName);
                    trace.Add(referenceName, currName);
                    toExpand.Enqueue(reference);
                }


            }

            return infos;

        }

    }

}