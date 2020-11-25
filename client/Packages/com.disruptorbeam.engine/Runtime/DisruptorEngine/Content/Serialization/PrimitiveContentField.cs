//using System;
//using System.Collections;
//using System.Collections.Generic;
//using System.Linq;
//using Beamable.Common.Content;
//using Core.Serialization;
//using Core.Serialization.SmallerJSON;
//using UnityEngine;
//using UnityEngine.AddressableAssets;
//
//namespace DisruptorBeam.Content.Serialization
//{
//   public abstract class PrimitiveContentField<T> : JsonSerializable.ISerializable
//   {
//      protected const string DATA = "data";
//      protected const string LINK = "$link";
//      protected const string LINKS = "$links";
//      public T Value;
//      public bool UseInline = false;
//
//      public PrimitiveContentField()
//      {
//         // empty
//      }
//      public abstract void Serialize(JsonSerializable.IStreamSerializer s);
//
//   }
//
//   public class IntContentField : PrimitiveContentField<int>
//   {
//      public override void Serialize(JsonSerializable.IStreamSerializer s)
//      {
//         s.Serialize(DATA, ref Value);
//      }
//   }
//
//   public class IntListContentField : PrimitiveContentField<IEnumerable<int>>
//   {
//      public override void Serialize(JsonSerializable.IStreamSerializer s)
//      {
//         var set = Value.ToArray();
//         s.SerializeArray(DATA, ref set);
//      }
//   }
//
//   public class LongContentField : PrimitiveContentField<long>
//   {
//      public override void Serialize(JsonSerializable.IStreamSerializer s)
//      {
//         s.Serialize(DATA, ref Value);
//      }
//   }
//
//   public class ColorContentField : PrimitiveContentField<Color>
//   {
//      public override void Serialize(JsonSerializable.IStreamSerializer s)
//      {
//         s.Serialize(DATA, ref Value);
//      }
//   }
//
//   public class LongListContentField : PrimitiveContentField<IEnumerable<long>>
//   {
//      public override void Serialize(JsonSerializable.IStreamSerializer s)
//      {
//         var set = Value.ToArray();
//         s.SerializeArray(DATA, ref set);
//      }
//   }
//
//   public class StringContentField : PrimitiveContentField<string>
//   {
//      public override void Serialize(JsonSerializable.IStreamSerializer s)
//      {
//         s.Serialize(DATA, ref Value);
//      }
//   }
//
//   public class StringListContentField : PrimitiveContentField<IEnumerable<string>>
//   {
//      public override void Serialize(JsonSerializable.IStreamSerializer s)
//      {
//         var set = Value.ToArray();
//         s.SerializeArray(DATA, ref set);
//      }
//   }
//
//   public class ObjectListContentField : PrimitiveContentField<IEnumerable<object>>
//   {
//      public override void Serialize(JsonSerializable.IStreamSerializer s)
//      {
//         var objArray = Value.ToArray();
//         var objSet = new DataWrapper[objArray.Length];
//         for (int i = 0; i < objArray.Length; i++)
//         {
//            objSet[i] = new DataWrapper(){Value = objArray[i]};
//         }
//         s.SerializeArray(DATA, ref objSet);
//      }
//   }
//
//   public class BoolContentField : PrimitiveContentField<bool>
//   {
//      public override void Serialize(JsonSerializable.IStreamSerializer s)
//      {
//         s.Serialize(DATA, ref Value);
//      }
//   }
//
//   public class BoolListContentField : PrimitiveContentField<IEnumerable<bool>>
//   {
//      public override void Serialize(JsonSerializable.IStreamSerializer s)
//      {
//         var set = Value.ToArray();
//         s.SerializeArray(DATA, ref set);
//      }
//   }
//
//   public class DictContentField : PrimitiveContentField<ContentDictionary>
//   {
//        public override void Serialize(JsonSerializable.IStreamSerializer s)
//        {
//            IDictionary<string, object> set = Value.keyValues.ToDictionary(x => x.Key, x => (object)x.Value);
//            s.Serialize(DATA, ref set);
//        }
//   }
//
//   public class PointerContentField : PrimitiveContentField<ContentRef>
//   {
//      public override void Serialize(JsonSerializable.IStreamSerializer s)
//      {
//         s.Serialize(DATA, ref Value.Id);
//      }
//   }
//
//   public class PointerListContentField : PrimitiveContentField<IEnumerable<ContentRef>>
//   {
//      public override void Serialize(JsonSerializable.IStreamSerializer s)
//      {
//         var set = Value.Select(l => l.Id).ToArray();
//         s.SerializeArray(DATA, ref set);
//      }
//   }
//
//   public class LinkContentField : PrimitiveContentField<ContentRef>
//   {
//      public override void Serialize(JsonSerializable.IStreamSerializer s)
//      {
//         s.Serialize(LINK, ref Value.Id);
//      }
//   }
//
//   public class LinksContentField : PrimitiveContentField<IEnumerable<ContentRef>>
//   {
//      public override void Serialize(JsonSerializable.IStreamSerializer s)
//      {
//         var set = Value.Select(l => l.Id).ToArray();
//         s.SerializeArray(LINKS, ref set);
//      }
//   }
//
//   public class DataContentField : PrimitiveContentField<object>
//   {
//      public override void Serialize(JsonSerializable.IStreamSerializer s)
//      {
//         var wrapper = new DataWrapper{Value = Value };
//         s.Serialize(DATA, ref wrapper);
//      }
//   }
//
//   class DataWrapper : JsonSerializable.ISerializable
//   {
//      public object Value;
//      public void Serialize(JsonSerializable.IStreamSerializer s)
//      {
//         foreach (var field in ContentSerialization.MapFields(Value))
//         {
//            var value = field.Value;
//            if (value is Optional optional)
//            {
//               if (optional.HasValue)
//               {
//                  value = optional.GetValue();
//               }
//               else
//               {
//                  continue;
//               }
//            }
//            switch (value)
//            {
//               // Each field is either a DATA, a TEXT, or a LINK.
//
//               // CHECK FOR BASIC PRIMITIVES...
//               case int v:
//                  s.Serialize(field.Name, ref v);
//                  break;
//               case long v:
//                  s.Serialize(field.Name, ref v);
//                  break;
//               case string v:
//                  s.Serialize(field.Name, ref v);
//                  break;
//               case bool v:
//                  s.Serialize(field.Name, ref v);
//                  break;
//               case Color color:
//                  s.Serialize(field.Name, ref color);
//                  break;
//               case Enum e:
//                  var enumString = e.ToString();
//                  s.Serialize(field.Name, ref enumString);
//                  break;
//               case IEnumerable<string> strList:
//                  var strSet = strList.ToArray();
//                  s.SerializeArray(field.Name, ref strSet);
//                  break;
//               case IEnumerable<bool> boolList:
//                  var boolSet = boolList.ToArray();
//                  s.SerializeArray(field.Name, ref boolSet);
//                  break;
//               case IEnumerable<int> intList:
//                  var intSet = intList.ToArray();
//                  s.SerializeArray(field.Name, ref intSet);
//                  break;
//               case IEnumerable<object> objList:
//                  var objArray = objList.ToArray();
//                  var objSet = new DataWrapper[objArray.Length];
//                  for (int i = 0; i < objArray.Length; i++)
//                  {
//                     objSet[i] = new DataWrapper{Value = objArray[i]};
//                  }
//                  s.SerializeArray(field.Name, ref objSet);
//                  break;
//               case ContentRef reference:
//                  s.Serialize(field.Name, ref reference.Id);
//                  break;
//
//               case AssetReference assetReference:
//                  var content = new AssetReferenceContent()
//                  {
//                     ReferenceKey = assetReference.AssetGUID
//                  };
//                  s.Serialize(field.Name, ref content);
//                  break;
//               case object v:
//                  if (v.GetType().BaseType != typeof(System.Object))
//                  {
//                     Debug.LogError($"Nested content must not have a base type: {v.GetType().BaseType}. child={v.GetType()}" );
//                     continue;
//                  }
//
//                  var wrapped = new DataWrapper() {Value = v};
//                  s.Serialize(field.Name, ref wrapped);
//                  break;
//            }
//
//         }
//      }
//   }
//}