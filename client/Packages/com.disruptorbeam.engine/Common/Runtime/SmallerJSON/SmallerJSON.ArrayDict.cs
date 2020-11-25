// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
//
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//

using System;
using System.Collections;
using System.Collections.Generic;
using TKey = System.String;
using TValue = System.Object;

namespace Beamable.Serialization.SmallerJSON
{
   public class ArrayDict : IDictionary<TKey, TValue>, IDictionary
   {
      TKey[] _keys;
      TValue[] _values;
      int _size;

      const int DefaultCapacity = 4;

      public ArrayDict()
      {
         _keys = new TKey[DefaultCapacity];
         _values = new TValue[DefaultCapacity];
      }

      public void Add(TKey key, TValue value)
      {
         for(int i = 0; i < _size; ++i) {
            if(key == _keys[i]) {
               _values[i] = value;
               return;
            }
         }

         GrowIfNeeded(1);
         _keys[_size] = key;
         _values[_size] = value;
         ++_size;
      }

      public void AddUnchecked(TKey key, TValue value)
      {
         GrowIfNeeded(1);
         _keys[_size] = key;
         _values[_size] = value;
         ++_size;
      }

      void GrowIfNeeded (int newCount)
      {
         int minimumSize = _size + newCount;
         if (minimumSize > _keys.Length) {
            int capacity = System.Math.Max (System.Math.Max (_keys.Length * 2, DefaultCapacity), minimumSize);
            var newKeys = new TKey[capacity];
            Array.Copy(_keys, newKeys, _size);
            _keys = newKeys;

            var newValues = new TValue[capacity];
            Array.Copy(_values, newValues, _size);
            _values = newValues;
         }
      }

      #region IDictionary implementation
      public bool ContainsKey (TKey key)
      {
         for(int i = 0; i < _size; ++i) {
            if(key == _keys[i]) {
               return true;
            }
         }

         return false;
      }
      public bool Remove (TKey key)
      {
         for(int i = 0; i < _size; ++i) {
            if(key == _keys[i]) {
               --_size;
               _keys[i] = _keys[_size];
               _values[i] = _values[_size];
               return true;
            }
         }

         return false;
      }
      public bool TryGetValue (TKey key, out TValue value)
      {
         for(int i = 0; i < _size; ++i) {
            if(key == _keys[i]) {
               value = _values[i];
               return true;
            }
         }

         value = default(TValue);
         return false;
      }
      public TValue this [TKey key] {
         get {
            for(int i = 0; i < _size; ++i) {
               if(key == _keys[i]) {
                  return _values[i];
               }
            }
            return null;
         }
         set {
            Add(key, value);
         }
      }
      public ICollection<TKey> Keys {
         get {
            var result = new TKey[_size];
            Array.Copy(_keys, result, _size);
            return result;
         }
      }
      public ICollection<TValue> Values {
         get {
            var result = new TValue[_size];
            Array.Copy(_values, result, _size);
            return result;
         }
      }
      #endregion
      #region ICollection implementation
      public void Add (KeyValuePair<TKey, TValue> item)
      {
         Add(item.Key, item.Value);
      }
      public void Clear ()
      {
         _size = 0;
      }
      public bool Contains (KeyValuePair<TKey, TValue> item)
      {
         throw new NotImplementedException ();
      }
      public void CopyTo (KeyValuePair<TKey, TValue>[] array, int arrayIndex)
      {
         throw new NotImplementedException ();
      }
      public bool Remove (KeyValuePair<TKey, TValue> item)
      {
         throw new NotImplementedException ();
      }
      public int Count {
         get {
            return _size;
         }
      }
      public bool IsReadOnly {
         get {
            return false;
         }
      }
      #endregion

      #region IEnumerable implementation
      public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator ()
      {
         return new Enumerator(this);
      }
      #endregion
      #region IEnumerable implementation
      IEnumerator IEnumerable.GetEnumerator ()
      {
         return new Enumerator(this);
      }
      #endregion

      #region IDictionary implementation

      bool IDictionary.IsFixedSize { get { return false; } }
      bool IDictionary.IsReadOnly { get { return false; } }
      int ICollection.Count { get { return _size; } }
      bool ICollection.IsSynchronized { get { return false; } }
      object ICollection.SyncRoot { get { return this; } }

      ICollection IDictionary.Keys {
         get {
            var result = new TKey[_size];
            Array.Copy(_keys, result, _size);
            return result;
         }
      }

      ICollection IDictionary.Values {
         get {
            var result = new TValue[_size];
            Array.Copy(_values, result, _size);
            return result;
         }
      }

      object IDictionary.this[object key] {
         get {
            var typedKey = key as TKey;
            return typedKey != null ? this[typedKey] : null;
         }
         set {
             Add((TKey)key, value);
         }
      }

      void IDictionary.Add(object key, object value)
      {
         Add((TKey)key, value);
      }

      void IDictionary.Clear()
      {
         _size = 0;
      }

      IDictionaryEnumerator IDictionary.GetEnumerator()
      {
         return new Enumerator(this);
      }

      bool IDictionary.Contains(object key)
      {
         var typedKey = key as TKey;
         return typedKey != null && ContainsKey(typedKey);
      }

      void IDictionary.Remove(object key)
      {
         var typedKey = key as TKey;
         if(typedKey != null) {
            Remove(typedKey);
         }
      }

      void ICollection.CopyTo(Array array, int index)
      {
         throw new NotImplementedException();
      }
      #endregion

      public struct Enumerator : IEnumerator<KeyValuePair<TKey, TValue>>, IDictionaryEnumerator
      {
         readonly ArrayDict _dict;
         int _index;

         internal Enumerator(ArrayDict dict) {
            _dict = dict;
            _index = -1;
         }

         #region IEnumerator implementation
         public bool MoveNext ()
         {
            if (_index + 1 < _dict._size) {
               ++_index;
               return true;
            }

            return false;
         }
         public void Reset ()
         {
            _index = -1;
         }
         object IEnumerator.Current {
            get {
               return new KeyValuePair<TKey, TValue>(_dict._keys[_index], _dict._values[_index]);
            }
         }
         #endregion
         #region IDisposable implementation
         public void Dispose ()
         {
         }
         #endregion
         #region IEnumerator implementation
         public KeyValuePair<TKey, TValue> Current {
            get {
               return new KeyValuePair<TKey, TValue>(_dict._keys[_index], _dict._values[_index]);
            }
         }
         #endregion

         #region IDictionaryEnumerator implementation
         object IDictionaryEnumerator.Key {
            get {
               return _dict._keys[_index];
            }
         }

         object IDictionaryEnumerator.Value {
            get {
               return _dict._values[_index];
            }
         }

         DictionaryEntry IDictionaryEnumerator.Entry {
            get {
               return new DictionaryEntry(_dict._keys[_index], _dict._values[_index]);
            }
         }
         #endregion
      }
   }
}

