using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;

namespace Beamable.Common.Content
{

    [System.Serializable]
    public abstract class Optional
    {
        public bool HasValue;
        public abstract object GetValue();

        static Optional()
        {
           TypeDescriptor.AddAttributes(typeof(Optional), new TypeConverterAttribute(typeof(OptionalTypeConverter)));
        }

        public abstract void SetValue(object value);
        public abstract Type GetOptionalType();
    }

    public static class OptionalTypes
    {

       public static Optional<int> ToOptional(this int number)
       {
          return new Optional<int>{HasValue = true, Value = number};
       }
    }

    [System.Serializable]
    public class Optional<T> : Optional
    {
        public T Value;
        public override object GetValue()
        {
            return Value;
        }

        public override void SetValue(object value)
        {
           Value = (T) value;
           HasValue = true;
        }

        public override Type GetOptionalType()
        {
           return typeof(T);
        }
    }

    public class OptionalTypeConverter : TypeConverter {
       public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType)
       {
          if (typeof(Optional).IsAssignableFrom(destinationType))
          {
             return true;
          }
          return base.CanConvertTo(context, destinationType);
       }

       public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
       {
          if (value is long number)
          {
             return new OptionalInt { Value = (int)number, HasValue = true };
          }
          return base.ConvertTo(context, culture, value, destinationType);
       }
    }

    [System.Serializable]
    public class OptionalInt : Optional<int> { }
    [System.Serializable]
    public class OptionalList : Optional<List<int>> { }



    [System.Serializable]
    public class OptionalString : Optional<string> { }


    [System.Serializable]
    public class KVPair
    {
        public string Key;
        public string Value;
        public string GetKey()
        {
            return Key;
        }

        public string GetValue()
        {
            return Value;
        }
    }
}