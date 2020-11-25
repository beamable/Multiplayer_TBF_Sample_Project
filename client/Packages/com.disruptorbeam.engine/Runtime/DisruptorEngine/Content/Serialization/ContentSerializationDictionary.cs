using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Beamable.Serialization;

namespace Beamable.Content.Serialization
{
   public class ContentSerializationDictionary : JsonSerializable.ISerializable, IDictionary<string, JsonSerializable.ISerializable>
   {
      private Dictionary<string, JsonSerializable.ISerializable> _dictionary = new Dictionary<string, JsonSerializable.ISerializable>();

      public void Serialize(JsonSerializable.IStreamSerializer s)
      {
         foreach (var kvp in _dictionary)
         {
            var val = kvp.Value;
            s.SerializeInline(kvp.Key, ref val);
         }
      }

      public void SerializeAsData(JsonSerializable.IStreamSerializer s)
      {
         s.SerializeDictionary("DATA", ref _dictionary);
      }

      public IEnumerator<KeyValuePair<string, JsonSerializable.ISerializable>> GetEnumerator()
      {
         return _dictionary.GetEnumerator();
      }

      IEnumerator IEnumerable.GetEnumerator()
      {
         return GetEnumerator();
      }

      public void Add(KeyValuePair<string, JsonSerializable.ISerializable> item)
      {
         _dictionary.Add(item.Key, item.Value);
      }

      public void Clear()
      {
         _dictionary.Clear();
      }

      public bool Contains(KeyValuePair<string, JsonSerializable.ISerializable> item)
      {
         return _dictionary.Contains(item);
      }

      public void CopyTo(KeyValuePair<string, JsonSerializable.ISerializable>[] array, int arrayIndex)
      {
         throw new System.NotImplementedException();

      }

      public bool Remove(KeyValuePair<string, JsonSerializable.ISerializable> item)
      {
         return _dictionary.Remove(item.Key);
      }

      public int Count => _dictionary.Count;
      public bool IsReadOnly => false;
      public void Add(string key, JsonSerializable.ISerializable value)
      {
         _dictionary.Add(key, value);
      }

      public bool ContainsKey(string key)
      {
         return _dictionary.ContainsKey(key);
      }

      public bool Remove(string key)
      {
         return _dictionary.Remove(key);
      }

      public bool TryGetValue(string key, out JsonSerializable.ISerializable value)
      {
         return _dictionary.TryGetValue(key, out value);
      }

      public JsonSerializable.ISerializable this[string key]
      {
         get => _dictionary[key];
         set => _dictionary[key] = value;
      }

      public ICollection<string> Keys => _dictionary.Keys;
      public ICollection<JsonSerializable.ISerializable> Values => _dictionary.Values;
   }
}