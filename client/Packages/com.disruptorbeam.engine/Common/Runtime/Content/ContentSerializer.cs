using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
// Promise library
using Beamable.Serialization.SmallerJSON;
using Beamable.Content;
// pull into common
using UnityEngine;
using UnityEngine.AddressableAssets; // stub out

namespace Beamable.Common.Content
{
   public abstract class ContentSerializer<TContentBase>
   {

      protected string GetNullStringForType(Type argType)
      {
         if (typeof(IList).IsAssignableFrom(argType))
         {
            return "[]";
         }

         if (!typeof(string).IsAssignableFrom(argType) && argType.GetCustomAttribute<System.SerializableAttribute>() != null)
         {
            try
            {
               var defaultInstance = Activator.CreateInstance(argType);
               return SerializeArgument(defaultInstance, argType);
            }
            catch (MissingMethodException)
            {
               return "null";
            }
         }

         return "null";
      }

      protected string SerializeArgument(object arg, Type argType)
      {
         // JSONUtility will serialize objects correctly, but doesn't handle primitives well.
         if (arg == null)
         {
            return GetNullStringForType(argType);
         }

         switch (arg)
         {
            /* ARRAY TYPES... */
            case IList arr:
               var serializedArray = new PropertyValue[arr.Count];

               var index = 0;
               foreach (var elem in arr)
               {
                  var serializedElem = SerializeArgument(elem, elem?.GetType() ?? typeof(void));
                  serializedArray[index] = new PropertyValue { rawJson = serializedElem };
                  index++;
               }

               return Json.Serialize(serializedArray, new StringBuilder());

            /* PRIMITIVE TYPES... */
            case Enum e:
               return Json.Serialize(arg, new StringBuilder());
            case bool b:
            case long l:
            case string s:
            case double d:
            case float f:
            case short sh:
            case byte by:
            case int i:
               return Json.Serialize(arg, new StringBuilder());

            /* SPECIAL TYPES... */
            case IContentRef contentRef:
               return Json.Serialize(new ArrayDict
               {
                  {"id", contentRef.GetId()}
               }, new StringBuilder());
            case AssetReference addressable:
               var addressableDict = new ArrayDict
               {
                  {"referenceKey", addressable.AssetGUID},
               };
               if (addressable.SubObjectName != null)
               {
                  addressableDict.Add("subObjectName", addressable.SubObjectName);
               }

               return Json.Serialize(addressableDict, new StringBuilder());

            default:

               /*
                * We can't use the JsonUtility.ToJson because we can't override certain types,
                *  like optionals, addressables, links or refs.
                */
               var fields = GetFieldInfos(arg.GetType());
               var dict = new ArrayDict();
               foreach (var field in fields)
               {
                  var fieldValue = field.GetValue(arg);
                  var fieldType = field.FieldType;

                  if (fieldValue is Optional optional)
                  {
                     if (optional.HasValue)
                     {
                        fieldValue = optional.GetValue();
                        fieldType = optional.GetOptionalType();
                     }
                     else
                     {
                        continue; // skip field.
                     }
                  }
                  var fieldJson = SerializeArgument(fieldValue, fieldType);
                  dict.Add(field.Name, new PropertyValue { rawJson = fieldJson });
               }

               return Json.Serialize(dict, new StringBuilder());
         }

      }


      protected object DeserializeResult(object preParsedValue, Type type)
      {

         if (typeof(Optional).IsAssignableFrom(type))
         {
            var optional = (Optional)Activator.CreateInstance(type);

            if (preParsedValue == null)
            {
               optional.HasValue = false;
            }
            else
            {
               var value = DeserializeResult(preParsedValue, optional.GetOptionalType());
               optional.SetValue(value);
            }

            return optional;
         }

         //if (typeof(IContentLink).IsAssignableFrom())


         var json = Json.Serialize(preParsedValue, new StringBuilder());

         if (typeof(Unit).IsAssignableFrom(type))
         {
            return PromiseBase.Unit;
         }

         switch (preParsedValue)
         {
            case IList list when type.IsArray:
               var output = (IList)Activator.CreateInstance(type, new object[] { list.Count });
               var fieldType = type.GetElementType();
               for (var index = 0; index < list.Count; index++)
               {
                  output[index] = DeserializeResult(list[index], fieldType);
               }
               return output;

            case IList list when type.GenericTypeArguments.Length == 1:
               var outputList = (IList)Activator.CreateInstance(type, new object[] { list.Count });
               var fieldTypeList = type.GenericTypeArguments[0];
               foreach (var elem in list)
               {
                  outputList.Add(DeserializeResult(elem, fieldTypeList));
               }
               return outputList;

            /* PRIMITIVES TYPES */
            case string enumValue when typeof(Enum).IsAssignableFrom(type):
               return Enum.Parse(type, enumValue);
            case string _:
               return json.Substring(1, json.Length - 2);
            case float _:
               return Convert.ChangeType(float.Parse(json), type);
            case long _:
               return Convert.ChangeType(long.Parse(json), type);
            case double _:
               return Convert.ChangeType(double.Parse(json), type);
            case bool _:
               return Convert.ChangeType(bool.Parse(json), type);
            case int _:
               return Convert.ChangeType(int.Parse(json), type);

            /* SPECIAL TYPES */
            case ArrayDict linkDict when typeof(IContentLink).IsAssignableFrom(type):
               var contentLink = (IContentLink)Activator.CreateInstance(type);
               object linkId = "";
               linkDict.TryGetValue("id", out linkId);
               contentLink.SetId(linkId?.ToString() ?? "");
               contentLink.OnCreated();
               return contentLink;
            case ArrayDict referenceDict when typeof(IContentRef).IsAssignableFrom(type):
               var contentRef = (IContentRef)Activator.CreateInstance(type);
               object id = "";
               referenceDict.TryGetValue("id", out id);
               contentRef.SetId(id?.ToString() ?? "");
               return contentRef;

            case ArrayDict assetDict when typeof(AssetReference).IsAssignableFrom(type):
               object guid = "";
               assetDict.TryGetValue("referenceKey", out guid);
               var assetRef = (AssetReference)Activator.CreateInstance(type, guid);
               if (assetDict.TryGetValue("subObjectName", out var subKey))
               {
                  assetRef.SubObjectName = subKey.ToString();
               }

               return assetRef;

            case ArrayDict dict:

               var fields = GetFieldInfos(type);
               var instance = Activator.CreateInstance(type);
               foreach (var field in fields)
               {
                  object fieldValue = null;
                  if (dict.TryGetValue(field.Name, out var property))
                  {
                     fieldValue = DeserializeResult(property, field.FieldType);
                  }
                  else
                  {
                     fieldValue = DeserializeResult(null, field.FieldType);
                  }
                  field.SetValue(instance, fieldValue);

               }

               return instance;
            default:
               throw new Exception($"Cannot deserialize type [{type.Name}]");
         }

      }
      public List<FieldInfo> GetFieldInfos(Type type)
      {
         var listOfPublicFields = type.GetFields(BindingFlags.Public | BindingFlags.Instance).ToList();
         var listOfPrivateFields = type.GetFields(BindingFlags.NonPublic | BindingFlags.Instance).Where(field =>
         {
            return field.GetCustomAttributes<SerializeField>() != null;
         });

         var serializableFields = listOfPublicFields.Union(listOfPrivateFields);
         var notIgnoredFields = serializableFields.Where(field => field.GetCustomAttribute<IgnoreContentFieldAttribute>() == null);

         return notIgnoredFields.ToList();
      }

      [System.Serializable]
      public class PropertyValue : IRawJsonProvider
      {
         public string rawJson;

         public string ToJson()
         {
            return rawJson;
         }
      }

      /// <summary>
      /// returns only the {} representing the properties object
      /// </summary>
      /// <param name="content"></param>
      /// <typeparam name="TContent"></typeparam>
      /// <returns></returns>
      public string SerializeProperties<TContent>(TContent content)
         where TContent : IContentObject

      {
         var fields = GetFieldInfos(content.GetType())
            .ToDictionary(f => f.Name);
         var propertyDict = new ArrayDict();

         foreach (var kvp in fields)
         {
            var fieldName = kvp.Key;
            var fieldInfo = kvp.Value;
            var fieldType = kvp.Value.FieldType;
            var fieldValue = fieldInfo.GetValue(content);
            var fieldDict = new ArrayDict();

            switch (fieldValue)
            {

               case IList list when
                  (list.GetType().GetGenericArguments().Length == 1 && typeof(IContentLink).IsAssignableFrom(list.GetType().GetGenericArguments()[0])) ||
                  (list.GetType().IsArray && typeof(IContentLink).IsAssignableFrom(list.GetType().GetElementType())):
                  var linkSet = new string[list.Count];
                  for (var i = 0; i < list.Count; i++)
                  {
                     var link = (IContentLink)list[i];
                     linkSet[i] = link.GetId();
                  }
                  fieldDict.Add("$links", linkSet);
                  propertyDict.Add(fieldName, fieldDict);
                  break;

               case IContentLink link:
                  fieldDict.Add("$link", link.GetId());
                  propertyDict.Add(fieldName, fieldDict);
                  break;
               default: // data block.
                  if (fieldValue is Optional optional)
                  {
                     if (optional.HasValue)
                     {
                        fieldValue = optional.GetValue();
                        fieldType = optional.GetOptionalType();
                     }
                     else
                     {
                        continue;
                     }
                  }
                  var jsonValue = SerializeArgument(fieldValue, fieldType);
                  fieldDict.Add("data", new PropertyValue { rawJson = jsonValue });
                  propertyDict.Add(fieldName, fieldDict);
                  break;
            }
         }


         var json = Json.Serialize(propertyDict, new StringBuilder());
         return json;
      }

      /// <summary>
      /// Returns the {id: 1, version: 1, properties: {}} json model.
      /// </summary>
      /// <param name="content"></param>
      /// <typeparam name="TContent"></typeparam>
      /// <returns></returns>
      public string Serialize<TContent>(TContent content)
         where TContent : IContentObject, new()
      {
         var fields = GetFieldInfos(content.GetType())
            .ToDictionary(f => f.Name);

         var contentDict = new ArrayDict
         {
            {"id", content.Id},
            {"version", content.Version ?? ""}
         };

         var propertyDict = new PropertyValue { rawJson = SerializeProperties(content) };
         contentDict.Add("properties", propertyDict);

         var json = Json.Serialize(contentDict, new StringBuilder());
         return json;
      }


      protected abstract TContent CreateInstance<TContent>() where TContent : TContentBase, IContentObject, new();
      public TContentBase DeserializeByType(string json, Type contentType)
      {
         return (TContentBase)GetType()
            .GetMethod(nameof(Deserialize))
            .MakeGenericMethod(contentType)
            .Invoke(this, new[] { json });
      }
      public TContent Deserialize<TContent>(string json)
         where TContent : TContentBase, IContentObject, new()
      {
         var instance = CreateInstance<TContent>();

         var fields = GetFieldInfos(typeof(TContent));
         var root = Json.Deserialize(json) as ArrayDict;

         var id = root["id"];
         var version = root["version"];

         var properties = root["properties"] as ArrayDict;
         instance.SetIdAndVersion(id.ToString(), version.ToString());

         foreach (var field in fields)
         {
            var fieldName = field.Name;
            if (!properties.TryGetValue(fieldName, out var property))
            {
               // mark empty optional, if exists.
               if (typeof(Optional).IsAssignableFrom(field.FieldType))
               {
                  var optional = Activator.CreateInstance(field.FieldType);
                  field.SetValue(instance, optional);
               }
               continue; // there is no property for this field...
            }

            if (property is ArrayDict propertyDict)
            {
               if (propertyDict.TryGetValue("data", out var dataValue))
               {
                  var hackResult = DeserializeResult(dataValue, field.FieldType);
                  field.SetValue(instance, hackResult);
               }

               if (propertyDict.TryGetValue("$link", out var linkValue) || propertyDict.TryGetValue("link", out linkValue))
               {
                  if (!typeof(IContentLink).IsAssignableFrom(field.FieldType))
                  {
                     throw new Exception($"Cannot deserialize a link into a field that isnt a link field=[{field.Name}] type=[{field.FieldType}]");
                  }

                  var link = (IContentLink)Activator.CreateInstance(field.FieldType);
                  link.SetId(linkValue.ToString());
                  link.OnCreated();
                  field.SetValue(instance, link);
               }

               if (propertyDict.TryGetValue("$links", out var linksValue) ||
                   propertyDict.TryGetValue("links", out linksValue))
               {

                  var set = (IList<object>)linksValue;
                  var links = Activator.CreateInstance(field.FieldType, set.Count);

                  var linkList = (IList)links;
                  Type elemType;
                  if (field.FieldType.IsArray)
                  {
                     elemType = field.FieldType.GetElementType();
                  }
                  else if (field.FieldType.GenericTypeArguments.Length == 1)
                  {
                     elemType = field.FieldType.GenericTypeArguments[0];
                  }
                  else
                  {
                     throw new Exception("Unknown link list type");
                  }

                  for (var i = 0; i < set.Count; i++)
                  {
                     var elem = (IContentLink)Activator.CreateInstance(elemType);
                     elem.SetId(set[i].ToString());
                     elem.OnCreated();

                     if (linkList.Count <= i)
                     {
                        linkList.Add(elem);
                     }
                     else
                     {
                        linkList[i] = elem;

                     }
                  }

                  field.SetValue(instance, links);

               }
            }
         }


         return instance;
      }
   }
}