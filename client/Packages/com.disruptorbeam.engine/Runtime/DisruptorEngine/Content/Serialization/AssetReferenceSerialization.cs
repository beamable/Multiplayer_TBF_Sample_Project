//using Core.Serialization;
//using UnityEngine.AddressableAssets;
//
//namespace DisruptorBeam.Content.Serialization
//{
//   public class AssetReferenceContent : JsonSerializable.ISerializable
//   {
//      public string ReferenceKey;
//      // TODO we could put other stuff here, like type, or whatnot...
//      public void Serialize(JsonSerializable.IStreamSerializer s)
//      {
//         s.Serialize(nameof(ReferenceKey), ref ReferenceKey);
//      }
//   }
//
//   public class AssetReferenceField : PrimitiveContentField<AssetReference>
//   {
//      public override void Serialize(JsonSerializable.IStreamSerializer s)
//      {
//         var key = Value.AssetGUID;
//         var content = new AssetReferenceContent
//         {
//            ReferenceKey = key
//         };
//         s.Serialize(DATA, ref content);
//      }
//   }
//}