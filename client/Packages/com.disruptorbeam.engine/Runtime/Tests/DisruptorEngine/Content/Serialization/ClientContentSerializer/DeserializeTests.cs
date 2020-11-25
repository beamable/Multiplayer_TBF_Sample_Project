using System;
using System.Collections.Generic;
using Beamable.Common.Content;
using Beamable.Tests.Content.Serialization.Support;
using Beamable.Content;
using Beamable.Content.Serialization;
using Beamable.UI;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace Beamable.Tests.Content.Serialization.ClientContentSerializationTests
{
   public class DeserializeTests
   {
      [Test]
      public void Primitives()
      {
         var json = @"{
   ""id"": ""test.nothing"",
   ""version"": ""123"",
   ""properties"": {
      ""text"": { ""data"": ""testtext"" },
      ""number"": { ""data"": 3.21 },
      ""longNumber"": { ""data"": 123 },
      ""flag"": { ""data"": true },
   },
}";

         var s = new TestSerializer();
         var o = s.Deserialize<TestContent>(json);

         Assert.AreEqual(true, o.flag);
         Assert.AreEqual(123, o.longNumber);
         Assert.AreEqual(true, Mathf.Abs(o.number - 3.21f) < .001f);
         Assert.AreEqual("testtext", o.text);
      }

      [Test]
      public void IdAndVerion()
      {
         var json = @"{
   ""id"": ""test.nothing"",
   ""version"": ""123"",
   ""properties"": {
      ""text"": { ""data"": ""testtext"" },
      ""number"": { ""data"": 3.21 },
      ""longNumber"": { ""data"": 123 },
      ""flag"": { ""data"": true },
   },
}";

         var s = new TestSerializer();
         var o = s.Deserialize<TestContent>(json);

         Assert.AreEqual("test.nothing", o.Id);
         Assert.AreEqual("123", o.Version);
      }

      [Test]
      public void Nested()
      {
         var json = @"{
   ""id"": ""test.nothing"",
   ""version"": ""123"",
   ""properties"": {
      ""sub"": { ""data"": {
            ""text"": ""testtext"",
            ""number"": 3.21,
            ""longNumber"": 123,
            ""flag"": true
         }
      },
   },
}";

         var s = new TestSerializer();
         var o = s.Deserialize<TestContentComplex>(json);

         Assert.AreEqual(true, o.sub.flag);
         Assert.AreEqual(123, o.sub.longNumber);
         Assert.AreEqual(true, Mathf.Abs(o.sub.number - 3.21f) < .001f);
         Assert.AreEqual("testtext", o.sub.text);
      }

      [Test]
      public void OptionalWithValue()
      {
         var json = @"{
   ""id"": ""test.nothing"",
   ""version"": ""123"",
   ""properties"": {
      ""maybeNumber"": { ""data"": 5 }
   },
}";

         var s = new TestSerializer();
         var o = s.Deserialize<TestOptional>(json);

         Assert.AreEqual(true, o.maybeNumber.HasValue);
         Assert.AreEqual(5, o.maybeNumber.Value);
      }

      [Test]
      public void OptionalNestedWithValue()
      {
         var json = @"{
   ""id"": ""test.nothing"",
   ""version"": ""123"",
   ""properties"": {
      ""sub"": { ""data"": { ""maybeNumber"": 5} }
   },
}";

         var s = new TestSerializer();
         var o = s.Deserialize<NestedOptional>(json);

         Assert.AreEqual(true, o.sub.maybeNumber.HasValue);
         Assert.AreEqual(5, o.sub.maybeNumber.Value);
      }

      [Test]
      public void OptionalNestedWithoutValue()
      {
         var json = @"{
   ""id"": ""test.nothing"",
   ""version"": ""123"",
   ""properties"": {
      ""sub"": { ""data"": { } }
   },
}";

         var s = new TestSerializer();
         var o = s.Deserialize<NestedOptional>(json);

         Assert.AreEqual(false, o.sub.maybeNumber.HasValue);
      }

      [Test]
      public void OptionalWithoutValue()
      {
         var json = @"{
   ""id"": ""test.nothing"",
   ""version"": ""123"",
   ""properties"": {

   },
}";

         var s = new TestSerializer();
         var o = s.Deserialize<TestOptional>(json);

         Assert.AreEqual(false, o.maybeNumber.HasValue);
      }

      [Test]
      public void Color()
      {
         var json = @"{
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
}";

         var s = new TestSerializer();
         var o = s.Deserialize<ColorContent>(json);

         Assert.AreEqual(1, o.color.r);
         Assert.AreEqual(0, o.color.g);
         Assert.AreEqual(0, o.color.b);
         Assert.AreEqual(1, o.color.a);
      }

      [Test]
      public void Ref()
      {
         var json = @"{
   ""id"": ""test.nothing"",
   ""version"": """",
   ""properties"": {
      ""reference"": {
         ""data"": {
            ""id"":""primitive.foo"",
         }
      }
   }
}";

         var s = new TestSerializer();
         var o = s.Deserialize<RefContent>(json);

         Assert.AreEqual("primitive.foo", o.reference.GetId());
      }

      [Test]
      public void RefNested()
      {
         var json = @"{
   ""id"": ""test.nothing"",
   ""version"": """",
   ""properties"": {
      ""sub"": {
         ""data"": {
            ""reference"": {""id"": ""primitive.foo""},
         }
      }
   }
}";

         var s = new TestSerializer();
         var o = s.Deserialize<NestedRefContent>(json);

         Assert.AreEqual("primitive.foo", o.sub.reference.GetId());
      }

      [Test]
      public void Link()
      {
         var json = @"{
   ""id"": ""test.nothing"",
   ""version"": """",
   ""properties"": {
      ""link"": {
         ""$link"": ""primitive.foo""
      }
   }
}";

         var s = new TestSerializer();
         var o = s.Deserialize<LinkContent>(json);

         Assert.AreEqual("primitive.foo", o.link.GetId());
         Assert.AreEqual(true, o.link.WasCreated);
      }

      [Test]
      public void LinkNested()
      {
         var json = @"{
   ""id"": ""test.nothing"",
   ""version"": """",
   ""properties"": {
      ""sub"": {
         ""data"": {""link"": {""id"": ""primitive.foo""}}
      }
   }
}";

         var s = new TestSerializer();
         var o = s.Deserialize<LinkNestedContent>(json);

         Assert.AreEqual("primitive.foo", o.sub.link.GetId());
         Assert.AreEqual(true, o.sub.link.WasCreated);
      }

      [Test]
      public void LinkNestedArray()
      {
         var json = @"{
   ""id"": ""test.nothing"",
   ""version"": """",
   ""properties"": {
      ""sub"": {
         ""data"": {""links"": [{""id"": ""primitive.foo""}, {""id"": ""primitive.foo2""}]}
      }
   }
}";

         var s = new TestSerializer();
         var o = s.Deserialize<LinkArrayNestedContent>(json);

         Assert.AreEqual(2, o.sub.links.Length);
         Assert.AreEqual("primitive.foo", o.sub.links[0].GetId());
         Assert.AreEqual("primitive.foo2", o.sub.links[1].GetId());
         Assert.AreEqual(true, o.sub.links[0].WasCreated);
         Assert.AreEqual(true, o.sub.links[1].WasCreated);
      }

      [Test]
      public void LinkArray()
      {
         var json = @"{
   ""id"": ""test.nothing"",
   ""version"": """",
   ""properties"": {
      ""links"": {
         ""$links"": [""primitive.foo"", ""primitive.foo2""]
      }
   }
}";

         var s = new TestSerializer();
         var o = s.Deserialize<LinkArrayContent>(json);

         Assert.AreEqual(2, o.links.Length);
         Assert.AreEqual("primitive.foo", o.links[0].GetId());
         Assert.AreEqual(true, o.links[0].WasCreated);
         Assert.AreEqual("primitive.foo2", o.links[1].GetId());
         Assert.AreEqual(true, o.links[1].WasCreated);
      }

      [Test]
      public void LinkList()
      {
         var json = @"{
   ""id"": ""test.nothing"",
   ""version"": """",
   ""properties"": {
      ""links"": {
         ""$links"": [""primitive.foo"", ""primitive.foo2""]
      }
   }
}";

         var s = new TestSerializer();
         var o = s.Deserialize<LinkListContent>(json);

         Assert.AreEqual(2, o.links.Count);
         Assert.AreEqual("primitive.foo", o.links[0].GetId());
         Assert.AreEqual(true, o.links[0].WasCreated);
         Assert.AreEqual("primitive.foo2", o.links[1].GetId());
         Assert.AreEqual(true, o.links[1].WasCreated);
      }


      [Test]
      public void ArrayNumber()
      {
         var json = @"{
   ""id"": ""test.nothing"",
   ""version"": """",
   ""properties"": {
      ""numbers"": {
         ""data"": [1,2,3]
      }
   }
}";

         var s = new TestSerializer();
         var o = s.Deserialize<ArrayNumberContent>(json);

         Assert.AreEqual(3, o.numbers.Length);
         Assert.AreEqual(1, o.numbers[0]);
         Assert.AreEqual(2, o.numbers[1]);
         Assert.AreEqual(3, o.numbers[2]);
      }

      [Test]
      public void ArrayNestedNumber()
      {
         var json = @"{
   ""id"": ""test.nothing"",
   ""version"": """",
   ""properties"": {
      ""sub"": {
         ""data"": {""numbers"":[1,2,3]}
      }
   }
}";

         var s = new TestSerializer();
         var o = s.Deserialize<NestedArrayNumberContent>(json);

         Assert.AreEqual(3, o.sub.numbers.Length);
         Assert.AreEqual(1, o.sub.numbers[0]);
         Assert.AreEqual(2, o.sub.numbers[1]);
         Assert.AreEqual(3, o.sub.numbers[2]);
      }

      [Test]
      public void ListNumber()
      {
         var json = @"{
   ""id"": ""test.nothing"",
   ""version"": """",
   ""properties"": {
      ""numbers"": {
         ""data"": [1,2,3]
      }
   }
}";

         var s = new TestSerializer();
         var o = s.Deserialize<ListNumberContent>(json);

         Assert.AreEqual(3, o.numbers.Count);
         Assert.AreEqual(1, o.numbers[0]);
         Assert.AreEqual(2, o.numbers[1]);
         Assert.AreEqual(3, o.numbers[2]);
      }

      [Test]
      public void Addressable()
      {
         var fakeGuid = Guid.NewGuid().ToString();
         var json = @"{
   ""id"": ""test.nothing"",
   ""version"": """",
   ""properties"": {
      ""sprite"": {
         ""data"": {
            ""referenceKey"": """ + fakeGuid +@""",
            ""subObjectName"":""tuna""}
      }
   }
}";

         var s = new TestSerializer();
         var o = s.Deserialize<SpriteAddressableContent>(json);

         Assert.AreEqual(fakeGuid, o.sprite.AssetGUID);
         Assert.AreEqual("tuna", o.sprite.SubObjectName);
      }

      [Test]
      public void Enum()
      {
         var json = @"{
   ""id"": ""test.nothing"",
   ""version"": """",
   ""properties"": {
      ""e"": {
         ""data"": ""B""
      }
   }
}";

         var s = new TestSerializer();
         var o = s.Deserialize<EnumContent>(json);

         Assert.AreEqual(TestEnum.B, o.e);
      }

#pragma warning disable CS0649

      class TestContent : TestContentObject
      {
         public string text;
         public float number;
         public long longNumber;
         public bool flag;
      }

      class TestContentComplex : TestContentObject
      {
         public TestContent sub;
      }

      class TestOptional : TestContentObject
      {
         public OptionalInt maybeNumber;
      }

      class NestedOptional : TestContentObject
      {
         public TestOptional sub;
      }

      class ColorContent : TestContentObject
      {
         public Color color;
      }

      class PrimitiveRef : TestContentRef<TestContent> {}
      class PrimitiveLink : TestContentLink<TestContent> {}

      class RefContent : TestContentObject
      {
         public PrimitiveRef reference;
      }

      class NestedRefContent : TestContentObject
      {
         public RefContent sub;
      }

      class ListNumberContent : TestContentObject
      {
         public List<int> numbers;
      }

      class ArrayNumberContent : TestContentObject
      {
         public int[] numbers;
      }

      class NestedArrayNumberContent : TestContentObject
      {
         public ArrayNumberContent sub;
      }

      class SpriteAddressableContent : TestContentObject
      {
         public AssetReferenceSprite sprite;
      }

      class LinkContent : TestContentObject
      {
         public PrimitiveLink link;
      }

      class LinkArrayContent : TestContentObject
      {
         public PrimitiveLink[] links;
      }
      class LinkListContent : TestContentObject
      {
         public List<PrimitiveLink> links;
      }

      class LinkNestedContent : TestContentObject
      {
         public LinkContent sub;
      }
      class LinkArrayNestedContent : TestContentObject
      {
         public LinkArrayContent sub;
      }

      enum TestEnum { A, B, C}

      class EnumContent : TestContentObject
      {
         public TestEnum e;
      }
#pragma warning restore CS0649

   }
}