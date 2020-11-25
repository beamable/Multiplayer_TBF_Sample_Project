using System;
using System.Collections.Generic;
using Beamable.Common.Content;
using Beamable.Tests.Content.Serialization.Support;
using Beamable.Content;
using Beamable.Content.Serialization;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace Beamable.Tests.Content.Serialization.ClientContentSerializationTests
{
   public class SerializeTests
   {
      [Test]
      public void Primitives()
      {
         var c = new PrimitiveContent
         {
            Id = "test.nothing",
            x = 3,
            b = true,
            s = "test",
            f = 3.2f,
            d = 3.4,
            l = 101
         };
         var expected = @"{
   ""id"": ""test.nothing"",
   ""version"": """",
   ""properties"": {
      ""x"": { ""data"": 3 },
      ""b"": { ""data"": true },
      ""s"": { ""data"": ""test"" },
      ""f"": { ""data"": 3.2 },
      ""d"": { ""data"": 3.4 },
      ""l"": { ""data"": 101 }
   }
}".Replace(Environment.NewLine, "").Replace(" ", "");


         var s = new TestSerializer();
         var json = s.Serialize(c);

         Assert.AreEqual(expected, json);

      }

      [Test]
      public void IdAndVersion()
      {
         var c = new PrimitiveContent
         {
            Id = "test.nothing",
            Version = "123",
            x = 3,
            b = true,
            s = "test",
            f = 3.2f,
            d = 3.4,
            l = 101
         };
         var expected = @"{
   ""id"": ""test.nothing"",
   ""version"": ""123"",
   ""properties"": {
      ""x"": { ""data"": 3 },
      ""b"": { ""data"": true },
      ""s"": { ""data"": ""test"" },
      ""f"": { ""data"": 3.2 },
      ""d"": { ""data"": 3.4 },
      ""l"": { ""data"": 101 }
   }
}".Replace(Environment.NewLine, "").Replace(" ", "");


         var s = new TestSerializer();
         var json = s.Serialize(c);

         Assert.AreEqual(expected, json);

      }

      [Test]
      public void Nested()
      {
         var c = new NestedContent
         {
            Id = "test.nothing",
            sub = new PrimitiveContent {
               x = 3,
               b = true,
               s = "test",
               f = 3.2f,
               d = 3.4,
               l = 101
            }
         };
         var expected = @"{
   ""id"": ""test.nothing"",
   ""version"": """",
   ""properties"": {
      ""sub"": { ""data"": {
         ""x"":  3,
         ""b"":  true,
         ""s"":  ""test"",
         ""f"":  3.2,
         ""d"":  3.4,
         ""l"":  101
      } }
   }
}".Replace(Environment.NewLine, "").Replace(" ", "");


         var s = new TestSerializer();
         var json = s.Serialize(c);

         Assert.AreEqual(expected, json);

      }

      [Test]
      public void OptionalWithValue()
      {
         var c = new OptionalContent
         {
            Id = "test.nothing",
            maybeNumber = new OptionalInt { HasValue = true, Value = 32}
         };
         var expected = @"{
   ""id"": ""test.nothing"",
   ""version"": """",
   ""properties"": {
      ""maybeNumber"": { ""data"": 32 }
   }
}".Replace(Environment.NewLine, "").Replace(" ", "");

         var s = new TestSerializer();
         var json = s.Serialize(c);

         Assert.AreEqual(expected, json);
      }

      [Test]
      public void OptionalWithoutValue()
      {
         var c = new OptionalContent
         {
            Id = "test.nothing",
            maybeNumber = new OptionalInt { HasValue = false, Value = 32}
         };
         var expected = @"{
   ""id"": ""test.nothing"",
   ""version"": """",
   ""properties"": {
   }
}".Replace(Environment.NewLine, "").Replace(" ", "");

         var s = new TestSerializer();
         var json = s.Serialize(c);

         Assert.AreEqual(expected, json);
      }

      [Test]
      public void OptionalNestedWithValue()
      {
         var c = new NestedOptionalContent
         {
            Id = "test.nothing",
            sub = new OptionalContent
            {
               Id = "sub.nothing",
               maybeNumber = new OptionalInt { HasValue = true, Value = 30}
            }
         };
         var expected = @"{
   ""id"": ""test.nothing"",
   ""version"": """",
   ""properties"": {
      ""sub"": {
         ""data"": {""maybeNumber"": 30}
      }
   }
}".Replace(Environment.NewLine, "").Replace(" ", "");

         var s = new TestSerializer();
         var json = s.Serialize(c);

         Assert.AreEqual(expected, json);
      }

      [Test]
      public void OptionalNestedWithoutValue()
      {
         var c = new NestedOptionalContent
         {
            Id = "test.nothing",
            sub = new OptionalContent
            {
               Id = "sub.nothing",
               maybeNumber = new OptionalInt { HasValue = false, Value = 30}
            }
         };
         var expected = @"{
   ""id"": ""test.nothing"",
   ""version"": """",
   ""properties"": {
      ""sub"": {
         ""data"": {}
      }
   }
}".Replace(Environment.NewLine, "").Replace(" ", "");

         var s = new TestSerializer();
         var json = s.Serialize(c);

         Assert.AreEqual(expected, json);
      }

      [Test]
      public void Color()
      {
         var c = new ColorContent
         {
            Id = "test.nothing",
            color = new Color(1f, 0f, 0f)
         };
         var expected = @"{
   ""id"": ""test.nothing"",
   ""version"": """",
   ""properties"": {
      ""color"": {
         ""data"": {
            ""r"":1,
            ""g"":0,
            ""b"":0,
            ""a"":1
         }
      }
   }
}".Replace(Environment.NewLine, "").Replace(" ", "");

         var s = new TestSerializer();
         var json = s.Serialize(c);

         Assert.AreEqual(expected, json);
      }

      [Test]
      public void Ref()
      {
         var c = new RefContent
         {
            Id = "test.nothing",
            reference = new PrimitiveRef { Id = "primitive.foo" }
         };
         var expected = @"{
   ""id"": ""test.nothing"",
   ""version"": """",
   ""properties"": {
      ""reference"": {
         ""data"": {
            ""id"": ""primitive.foo""
         }
      }
   }
}".Replace(Environment.NewLine, "").Replace(" ", "");

         var s = new TestSerializer();
         var json = s.Serialize(c);

         Assert.AreEqual(expected, json);
      }

      [Test]
      public void RefNested()
      {
         var c = new NestedRefContent
         {
            Id = "test.nothing",
            sub = new RefContent
            {
               reference = new PrimitiveRef { Id = "primitive.foo" }
            }
         };
         var expected = @"{
   ""id"": ""test.nothing"",
   ""version"": """",
   ""properties"": {
      ""sub"": {
         ""data"": {
            ""reference"": { ""id"": ""primitive.foo"" }
         }
      }
   }
}".Replace(Environment.NewLine, "").Replace(" ", "");

         var s = new TestSerializer();
         var json = s.Serialize(c);

         Assert.AreEqual(expected, json);
      }

      [Test]
      public void Link()
      {
         var c = new LinkContent
         {
            Id = "test.nothing",
            link = new PrimitiveLink { Id = "primitive.foo" }
         };
         var expected = @"{
   ""id"": ""test.nothing"",
   ""version"": """",
   ""properties"": {
      ""link"": {
         ""$link"": ""primitive.foo""
      }
   }
}".Replace(Environment.NewLine, "").Replace(" ", "");

         var s = new TestSerializer();
         var json = s.Serialize(c);

         Assert.AreEqual(expected, json);
      }

      [Test]
      public void LinkNested()
      {
         var c = new LinkNestedContent
         {
            Id = "test.nothing",
            sub = new LinkContent { link = new PrimitiveLink { Id = "primitive.foo" } }
         };
         var expected = @"{
   ""id"": ""test.nothing"",
   ""version"": """",
   ""properties"": {
      ""sub"": {
         ""data"": {""link"": {""id"":""primitive.foo""} }
      }
   }
}".Replace(Environment.NewLine, "").Replace(" ", "");

         var s = new TestSerializer();
         var json = s.Serialize(c);

         Assert.AreEqual(expected, json);
      }

      [Test]
      public void LinkArray()
      {
         var c = new LinkArrayContent
         {
            Id = "test.nothing",
            links = new PrimitiveLink[]
            {
               new PrimitiveLink { Id = "primitive.foo" },
               new PrimitiveLink { Id = "primitive.foo2" },
            }
         };
         var expected = @"{
   ""id"": ""test.nothing"",
   ""version"": """",
   ""properties"": {
      ""links"": {
         ""$links"": [""primitive.foo"", ""primitive.foo2""]
      }
   }
}".Replace(Environment.NewLine, "").Replace(" ", "");

         var s = new TestSerializer();
         var json = s.Serialize(c);

         Assert.AreEqual(expected, json);
      }

      [Test]
      public void LinkList()
      {
         var c = new LinkListContent
         {
            Id = "test.nothing",
            links = new List<PrimitiveLink>
            {
               new PrimitiveLink { Id = "primitive.foo" },
               new PrimitiveLink { Id = "primitive.foo2" },
            }
         };
         var expected = @"{
   ""id"": ""test.nothing"",
   ""version"": """",
   ""properties"": {
      ""links"": {
         ""$links"": [""primitive.foo"", ""primitive.foo2""]
      }
   }
}".Replace(Environment.NewLine, "").Replace(" ", "");

         var s = new TestSerializer();
         var json = s.Serialize(c);

         Assert.AreEqual(expected, json);
      }

      [Test]
      public void ListNumbers()
      {
         var c = new NumberListContent
         {
            Id = "test.nothing",
            numbers = new List<int>{1,2,3}
         };
         var expected = @"{
   ""id"": ""test.nothing"",
   ""version"": """",
   ""properties"": {
      ""numbers"": {
         ""data"": [1,2,3]
      }
   }
}".Replace(Environment.NewLine, "").Replace(" ", "");

         var s = new TestSerializer();
         var json = s.Serialize(c);

         Assert.AreEqual(expected, json);
      }


      [Test]
      public void ArrayNumbers()
      {
         var c = new NumberArrayContent
         {
            Id = "test.nothing",
            numbers = new int[]{1,2,3}
         };
         var expected = @"{
   ""id"": ""test.nothing"",
   ""version"": """",
   ""properties"": {
      ""numbers"": {
         ""data"": [1,2,3]
      }
   }
}".Replace(Environment.NewLine, "").Replace(" ", "");

         var s = new TestSerializer();
         var json = s.Serialize(c);

         Assert.AreEqual(expected, json);
      }

      [Test]
      public void ArrayNestedNumbers()
      {
         var c = new NestedNumberArrayContent
         {
            Id = "test.nothing",
            sub = new NumberArrayContent {
               numbers = new int[]{1,2,3}
            }
         };
         var expected = @"{
   ""id"": ""test.nothing"",
   ""version"": """",
   ""properties"": {
      ""sub"": {
         ""data"": {""numbers"": [1,2,3]}
      }
   }
}".Replace(Environment.NewLine, "").Replace(" ", "");

         var s = new TestSerializer();
         var json = s.Serialize(c);

         Assert.AreEqual(expected, json);
      }

      [Test]
      public void Addressable()
      {
         var fakeGuid = Guid.NewGuid().ToString();
         var c = new SpriteAddressableContent
         {
            Id = "test.nothing",
            sprite = new AssetReferenceSprite(fakeGuid)
         };
         c.sprite.SubObjectName = "tuna";
         var expected = (@"{
   ""id"": ""test.nothing"",
   ""version"": """",
   ""properties"": {
      ""sprite"": {
         ""data"": {""referenceKey"": """ + fakeGuid + @""", ""subObjectName"": ""tuna""}
      }
   }
}").Replace(Environment.NewLine, "").Replace(" ", "");

         var s = new TestSerializer();
         var json = s.Serialize(c);

         Assert.AreEqual(expected, json);
      }

      [Test]
      public void Enum()
      {
         var c = new EnumContent
         {
            Id = "test.nothing",
            e = TestEnum.B
         };
         var expected = @"{
   ""id"": ""test.nothing"",
   ""version"": """",
   ""properties"": {
      ""e"": {
         ""data"": ""B""
      }
   }
}".Replace(Environment.NewLine, "").Replace(" ", "");

         var s = new TestSerializer();
         var json = s.Serialize(c);

         Assert.AreEqual(expected, json);
      }


      [Test]
      public void NullArray_Nested_SerializesAsEmpty()
      {
         var c = new NestedNumberArrayContent()
         {
            Id = "test.tuna",
            sub = new NumberArrayContent
            {
               numbers = null
            }
         };
         var expected = @"{
   ""id"": ""test.tuna"",
   ""version"": """",
   ""properties"": {
      ""sub"": {
         ""data"": {""numbers"":[]}
      }
   }
}".Replace(Environment.NewLine, "").Replace(" ", "");
         var s = new TestSerializer();
         var json = s.Serialize(c);
         Assert.AreEqual(expected, json);
      }

      [Test]
      public void NullArray_SerializesAsEmpty()
      {
         var c = new NumberArrayContent
         {
            Id = "test.tuna",
            numbers = null
         };
         var expected = @"{
   ""id"": ""test.tuna"",
   ""version"": """",
   ""properties"": {
      ""numbers"": {
         ""data"": []
      }
   }
}".Replace(Environment.NewLine, "").Replace(" ", "");
         var s = new TestSerializer();
         var json = s.Serialize(c);
         Assert.AreEqual(expected, json);
      }

      [Test]
      public void NullList_SerializesAsEmpty()
      {
         var c = new NumberListContent
         {
            Id = "test.tuna",
            numbers = null
         };
         var expected = @"{
   ""id"": ""test.tuna"",
   ""version"": """",
   ""properties"": {
      ""numbers"": {
         ""data"": []
      }
   }
}".Replace(Environment.NewLine, "").Replace(" ", "");
         var s = new TestSerializer();
         var json = s.Serialize(c);
         Assert.AreEqual(expected, json);
      }

      [Test]
      public void NullSerializable_SerializesWithDefaultInstance()
      {
         var c = new NestedContent
         {
            Id = "test.tuna",
            sub = null
         };
         var expected = @"{
   ""id"": ""test.tuna"",
   ""version"": """",
   ""properties"": {
      ""sub"": { ""data"": {
         ""x"":  0,
         ""b"":  false,
         ""s"":  null,
         ""f"":  0,
         ""d"":  0,
         ""l"":  0
      } }
   }
}".Replace(Environment.NewLine, "").Replace(" ", "");
         var s = new TestSerializer();
         var json = s.Serialize(c);
         Assert.AreEqual(expected, json);
      }


      [System.Serializable]
      class PrimitiveContent : TestContentObject
      {

         public int x;
         public bool b;
         public string s;
         public float f;
         public double d;
         public long l;
      }

      class NestedContent : TestContentObject
      {

         public PrimitiveContent sub;
      }

      class OptionalContent : TestContentObject
      {
         public OptionalInt maybeNumber;
      }

      class NestedOptionalContent : TestContentObject
      {
         public OptionalContent sub;
      }

      class ColorContent : TestContentObject
      {
         public Color color;
      }

      class PrimitiveRef : TestContentRef<PrimitiveContent>
      {

      }

      class PrimitiveLink : TestContentLink<PrimitiveContent>
      {

      }

      class RefContent : TestContentObject
      {
         public PrimitiveRef reference;
      }

      class NestedRefContent : TestContentObject
      {
         public RefContent sub;
      }

      class NumberArrayContent : TestContentObject
      {
         public int[] numbers;
      }

      class NestedNumberArrayContent : TestContentObject
      {
         public NumberArrayContent sub;
      }

      class NumberListContent : TestContentObject
      {
         public List<int> numbers;
      }

      class SpriteAddressableContent : TestContentObject
      {
         public AssetReferenceSprite sprite;
      }

      class LinkContent : TestContentObject
      {
         public PrimitiveLink link;
      }

      class LinkNestedContent : TestContentObject
      {
         public LinkContent sub;
      }

      class LinkArrayContent : TestContentObject
      {
         public PrimitiveLink[] links;
      }

      class LinkListContent : TestContentObject
      {
         public List<PrimitiveLink> links;
      }

      enum TestEnum
      {
         A, B, C
      }
      class EnumContent : TestContentObject
      {
         public TestEnum e;
      }
   }
}