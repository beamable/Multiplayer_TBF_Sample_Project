
using System.Collections;
using System.Collections.Generic;
using Beamable.Platform.SDK;
using Beamable;
using Beamable.Common;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace Beamable.Server
{
   public class MicroserviceClient
   {
      [System.Serializable]
      public class ResponseObject
      {
         public string payload;
      }

      [System.Serializable]
      public class RequestObject
      {
         public string payload;
      }

      protected string SerializeArgument<T>(T arg)
      {
         // JSONUtility will serialize objects correctly, but doesn't handle primitives well.
         if (arg == null)
         {
            return "undefined";
         }

         switch (arg)
         {
            case IEnumerable enumerable when !(enumerable is string):
               var output = new List<string>();
               foreach (var elem in enumerable)
               {
                  output.Add(SerializeArgument(elem));
               }

               var outputJson = "[" + string.Join(",", output) + "]";
               return outputJson;

            case bool prim:
               return prim ? "true": "false";
            case long prim:
               return prim.ToString();
            case string prim:
               return "\"" + prim + "\"";
            case double prim:
               return prim.ToString();
            case float prim:
               return prim.ToString();
            case int prim:
               return prim.ToString();
         }
         return JsonUtility.ToJson(arg);
      }

      protected T DeserializeResult<T>(string json)
      {
         var defaultInstance = default(T);

         if (typeof(Unit).IsAssignableFrom(typeof(T)))
         {
            return (T)(object) PromiseBase.Unit;
         }

         if (typeof(T).Equals(typeof(string)))
         {
            return (T)(object) json;
         }
         switch (defaultInstance)
         {
            case float _:
               return (T) (object) float.Parse(json);
            case long _:
               return (T) (object) long.Parse(json);
            case double _:
               return (T) (object) double.Parse(json);
            case bool _:
               return (T) (object) bool.Parse(json);
            case int _:
               return (T) (object) int.Parse(json);
         }

         return JsonUtility.FromJson<T>(json);
      }

      protected Promise<T> Request<T>(string endpoint, string[] serializedFields)
      {
         Debug.Log($"Client called {endpoint} with {serializedFields.Length} arguments");
         var argArray = "[ " + string.Join(",", serializedFields) + " ]";
         Debug.Log(argArray);

         return API.Instance.Map(de => de.Requester).FlatMap(requester =>
            {
               var url = $"/basic/{requester.Cid}.{requester.Pid}.{endpoint}";

               var req = new RequestObject
               {
                  payload = argArray
               };

               Debug.Log($"Sending Request uri=[{url}]");
               return requester.Request<ResponseObject>(Method.POST, url, req);
            })
            .Error(err => { Debug.LogError(err); })
            .Map(res =>
            {
               //Debug.Log("GOT A RESPONSE" + res);
               var result = DeserializeResult<T>(res.payload);
               return result;
            });
      }
   }
}