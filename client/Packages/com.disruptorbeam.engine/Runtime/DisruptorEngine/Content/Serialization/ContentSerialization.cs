//using System;
//using System.Collections;
//using System.Collections.Generic;
//using System.ComponentModel;
//using System.Linq;
//using System.Reflection;
//using Beamable.Common.Content;
//using Core.Serialization;
//using Core.Serialization.SmallerJSON;
//using UnityEngine;
//using UnityEngine.AddressableAssets;
//
//namespace DisruptorBeam.Content.Serialization
//{
//   public class ContentSerialization
//   {
//      internal static IEnumerable<ContentObjectField> MapFields(object target)
//      {
//         var type = target.GetType();
//         var fields = type.GetFields(BindingFlags.Instance | BindingFlags.Public);
//         foreach (var field in fields)
//         {
//            var fieldName = field.Name;
//            var fieldType = field.FieldType;
//
//            var fieldValue = field.GetValue(target);
//
//            yield return new ContentObjectField()
//            {
//               Name = fieldName,
//               Type = fieldType,
//               Value = fieldValue,
//               Setter = arg =>
//               {
//                  try
//                  {
//                     var convertedArg = Convert.ChangeType(arg, field.FieldType);
//                     field.SetValue(target, convertedArg);
//                  }
//                  catch (Exception ex)
//                  {
//                     var converter = TypeDescriptor.GetConverter(field.FieldType);
//                     var output = converter.ConvertTo(arg, field.FieldType);
//                     field.SetValue(target, output);
//                  }
//               }
//            };
//         }
//      }
//
//      internal static ContentSerializationDictionary ConvertToDictionary(object target)
//      {
//         var props = new ContentSerializationDictionary();
//         // get all fields of object....
//         foreach (var field in MapFields(target))
//         {
//            var value = field.Value;
//            if (value is Optional optional)
//            {
//                if (optional.HasValue)
//                {
//                    value = optional.GetValue();
//                }
//                else
//                {
//                    continue;
//                }
//            }
//                switch (value)
//            {
//                    // Each field is either a DATA, a TEXT, or a LINK.
//
//                    // CHECK FOR BASIC PRIMITIVES...
//               case string str:
//                  props.Add(field.Name, new StringContentField { Value = str} );
//                  break;
//               case bool b:
//                  props.Add(field.Name, new BoolContentField {Value = b} );
//                  break;
//               case int i:
//                  props.Add(field.Name, new IntContentField { Value = i} );
//                  break;
//               case long l:
//                  props.Add(field.Name, new LongContentField { Value = l});
//                  break;
//               case Color c:
//                  props.Add(field.Name, new ColorContentField {Value = c});
//                  break;
//               case Enum e:
//                  props.Add(field.Name, new StringContentField { Value = e.ToString()} );
//                  break;
//               case IEnumerable<string> strList:
//                  props.Add(field.Name, new StringListContentField {Value = strList});
//                  break;
//               case IEnumerable<bool> boolList:
//                  props.Add(field.Name, new BoolListContentField {Value = boolList});
//                  break;
//               case IEnumerable<int> intList:
//                  props.Add(field.Name, new IntListContentField {Value = intList});
//                  break;
//                case ContentDictionary pseudoDict:
//                  props.Add(field.Name, new DictContentField { Value = pseudoDict });
//                  break;
//               case ContentRef link when link is AutoResolveContent:
//                  props.Add(field.Name, new LinkContentField {Value = link});
//                  break;
//
//               case ContentRef reference:
//                  props.Add(field.Name, new PointerContentField{Value = reference});
//                  break;
//
//               case IEnumerable<ContentRef> links when typeof(AutoResolveContent).IsAssignableFrom(links.GetType().GetGenericArguments()[0]):
//                  props.Add(field.Name, new LinksContentField {Value = links});
//                  break;
//
//               case IEnumerable<ContentRef> references:
//                  props.Add(field.Name, new PointerListContentField { Value = references });
//                  break;
//
//               case IEnumerable<object> objects:
//                  props.Add(field.Name, new ObjectListContentField {Value = objects});
//                  break;
//
//               // TODO CHECK FOR CONTENTOBJECT TYPES...
//
//               // TODO CHECK FOR LOCALIZEDTEXT TYPES...
//
//               // CHECK FOR UNITY ADDRESSABLES
//               case AssetReference reference:
//                  props.Add(field.Name, new AssetReferenceField {Value = reference} );
//                  break;
//
//               // CHECK FOR COMPLEX OBJECT TYPES....
//               case object obj:
//                  // TODO check that the object doesnt inherit from anything else
//                  if (obj.GetType().BaseType != typeof(System.Object))
//                  {
//                     Debug.LogWarning($"Nested content must not have a base type: {obj.GetType().BaseType}. child={obj.GetType()}" );
//                     continue;
//                  }
//
//                  props.Add(field.Name, new DataContentField{Value = obj});
//                  break;
//               case null:
//                  // sad beans, no data.
//                  break;
//               default:
//
//                  throw new NotImplementedException();
//            }
//         }
//
//         return props;
//      }
//
//
//      internal static void ConvertFromDictionary(object target, ArrayDict dict)
//      {
//         /* the dictionary has a bunch of fields of the form
//         {
//            "someField": {
//               "data": ???,
//               "other": ???
//            }
//         }
//         For now, we only support the data field. Assume it exists.
//         After we have the data object, we need to cram it into the actual target.
//         It can either be a json primitive, or a json object.
//         */
//
//         void AcceptData(ContentObjectField field, object data)
//         {
//
//            switch (data)
//            {
//               case float f:
//               case bool b:
//               case int v:
//               case long l:
//               case double d:
//                  field.Setter(data);
//                  break;
//               case Color c:
//                  field.Setter(data);
//                  break;
//               case string contentId when typeof(ContentRef).IsAssignableFrom(field.Type):
//                  var instance = Activator.CreateInstance(field.Type);
//
//                  var reference = instance as ContentRef;
//                  reference.Id = contentId;
//
//                  field.Setter(instance);
//                  break;
//
//               case string s:
//                  field.Setter(data);
//                  break;
//               case IList subList when typeof(Color).IsAssignableFrom(field.Type):
//                  var color = new Color();
//                  Func<Action<float>, ContentObjectField> colorChannelGen = (set) =>
//                  {
//                     return new ContentObjectField
//                     {
//                        Name = "channel",
//                        Type = typeof(float),
//                        Setter = arg =>
//                        {
//                           var convertedArg = (float) Convert.ChangeType(arg, typeof(float));
//                           set(convertedArg);
//                        }
//                     };
//                  };
//                  AcceptData(colorChannelGen(v => color.r = v), subList[0]);
//                  AcceptData(colorChannelGen(v => color.g = v), subList[1]);
//                  AcceptData(colorChannelGen(v => color.b = v), subList[2]);
//                  AcceptData(colorChannelGen(v => color.a = v), subList[3]);
//                  field.Setter(color);
//                  break;
//               case IList subList:
//                  var subListInstance = Activator.CreateInstance(field.Type); // TODO we are enforcing new()
//                  var setList = subListInstance as IList;
//                  var genericArgs = field.Type.GetGenericArguments();
//                 // var itemType = genericArgs.Length > 0 ? genericArgs[0] : field.Type;
//                  var itemType = genericArgs[0];
//                  for(int i = 0; i < subList.Count; i++)
//                  {
//                        var item = subList[i];
//                        var itemField = new ContentObjectField()
//                        {
//                            Name = i.ToString(),
//                            Type = itemType,
//                            Setter = arg =>
//                            {
//                                var convertedArg = Convert.ChangeType(arg, itemType);
//                                setList.Add(convertedArg);
//                            }
//                        };
//                        AcceptData(itemField, item);
//                  }
//                  field.Setter(subListInstance);
//
//                  break;
//
//               case ArrayDict referenceDict when typeof(AssetReference).IsAssignableFrom(field.Type):
//
//                  AssetReferenceContent assetReferenceContent = new AssetReferenceContent();
//                  JsonSerializable.Deserialize(assetReferenceContent, referenceDict);
//
//                  var assetReference = Activator.CreateInstance(field.Type, assetReferenceContent.ReferenceKey);
//                  field.Setter(assetReference);
//
//                  break;
//
//               case ArrayDict subDict:
//                  var subInstance = field.Value;
//                  if (subInstance == null)
//                  {
//                     subInstance = Activator.CreateInstance(field.Type); // TODO we are enforcing new()
//                     field.Setter(subInstance);
//                  }
//
//                  var subFields = MapFields(subInstance).ToDictionary(f => f.Name);
//                  foreach (var kvp in subDict)
//                  {
//                     ContentObjectField subField;
//                     if (subFields.TryGetValue(kvp.Key, out subField))
//                     {
//                        AcceptData(subField, kvp.Value);
//                     }
//                  }
//
//                  break;
//            }
//         }
//
//         void AcceptLink(ContentObjectField field, string linkId)
//         {
//            // create the reference
//            if (!typeof(AutoResolveContent).IsAssignableFrom(field.Type))
//            {
//               throw new Exception("Must be a auto resolvable content");
//            }
//            var instance = Activator.CreateInstance(field.Type); // TODO we are enforcing new()
//
//            var reference = instance as ContentRef;
//            reference.Id = linkId;
//            if (Application.isPlaying)
//            {
//               var autoReference = instance as AutoResolveContent;
//               autoReference.AutoResolve();
//            }
//
//            field.Setter(instance);
//         }
//
//         void AcceptLinks(ContentObjectField field, IList<string> linkIds)
//         {
//            // field type must be an ICollection of some sort...
//            if (!typeof(IEnumerable<AutoResolveContent>).IsAssignableFrom(field.Type))
//            {
//               throw new Exception("Must be a a set of auto resolvable content");
//            }
//
//            if (!typeof(IList).IsAssignableFrom(field.Type))
//            {
//               throw new Exception("Must be a a set of auto resolvable content");
//            }
//
//            var setInstance = Activator.CreateInstance(field.Type); // TODO we are enforcing new()
//            var setList = setInstance as IList;
//
//            var enumerableInstance = setInstance as IEnumerable<AutoResolveContent>;
//            var autoResolveType = enumerableInstance.GetType().GetGenericArguments()[0];
//            if (!typeof(AutoResolveContent).IsAssignableFrom(autoResolveType))
//            {
//               throw new Exception("Must be a auto resolvable content");
//            }
//
//            foreach (var linkId in linkIds)
//            {
//               // create an instance of the autoResolvable...
//               var referenceInstance = Activator.CreateInstance(autoResolveType) as ContentRef;
//               referenceInstance.Id = linkId;
//               if (Application.isPlaying)
//               {
//                  var autoReference = referenceInstance as AutoResolveContent;
//                  autoReference.AutoResolve();
//               }
//
//               setList.Add(referenceInstance);
//            }
//            field.Setter(setInstance);
//         }
//
//
//
//         var fields = MapFields(target).ToDictionary(f => f.Name);
//         foreach (var kvp in dict)
//         {
//            ContentObjectField field;
//            if (fields.TryGetValue(kvp.Key, out field))
//            {
//               var allContent = (kvp.Value as ArrayDict);
//
//               // get data object.
//               object data = allContent?["data"];
//               if (data != null)
//               {
//                  AcceptData(field, data);
//               }
//
//               // get link data
//               object link = allContent?["$link"] ?? allContent?["link"];
//               if (link != null)
//               {
//                  var linkId = link as string;
//                  AcceptLink(field, linkId);
//               }
//
//               object links = allContent["$links"] ?? allContent["links"];
//               if (links != null)
//               {
//                  var objectLinkIds = links as IEnumerable<object>;
//                  var linkIds = objectLinkIds.ToList().ConvertAll(x => x.ToString());
//                  AcceptLinks(field, linkIds);
//               }
//            }
//         }
//      }
//   }
//}