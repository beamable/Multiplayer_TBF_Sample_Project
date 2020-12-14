using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Beamable.Common.Content;
using Beamable.Common.Content.Validation;
using Beamable.Content.Serialization;
using Beamable.Content.Validation;
using Beamable.Serialization.SmallerJSON;
using UnityEngine;

namespace Beamable.Content
{
   public struct ContentObjectField
   {
      public string Name;
      public Type Type;
      public object Value;
      public Action<object> Setter;
   }

   public delegate void ContentDelegate(ContentObject content);
   public delegate void IContentDelegate(IContentObject content);
   public delegate void IContentRenamedDelegate(string oldId, IContentObject content, string nextAssetPath);

   [System.Serializable]
   public class ContentObject : ScriptableObject, IContentObject, IRawJsonProvider
   {
      public event ContentDelegate OnChanged;

      [Obsolete]
      public string ContentVersion => Version;

      public string ContentName { get; private set; }

      public string ContentType => GetContentTypeName(GetType());
      public string Id => $"{ContentType}.{(string.IsNullOrEmpty(ContentName) ? name : ContentName)}";
      public string Version { get; private set; }

      [SerializeField]
      [IgnoreContentField]
      [HideInInspector]
      private string[] _tags;

      public string[] Tags
      {
         get => _tags ?? (_tags = new []{"base"});
         set => _tags = value;
      }

      public void SetIdAndVersion(string id, string version)
      {
         // validate id.
         var typeName = ContentType;
         if (!id.StartsWith(typeName))
         {
            throw new Exception($"Content type of [{typeName}] cannot use id=[{id}]");
         }

         ContentName = id.Substring(typeName.Length + 1); // +1 for the dot.
         Version = version;
      }

      public ContentObject SetContentName(string name)
      {
         if (ContentName != name || this.name != name)
         {
            ContentName = name;
            this.name = name;
         }
         return this;
      }
      public ContentObject SetContentMetadata(string name, string version)
      {
         ContentName = name;
         Version = version;
         return this;
      }


      public void BroadcastUpdate()
      {
         OnChanged?.Invoke(this);
      }


      /// <summary>
      /// Serialize this content into a json block, containing *ONLY* the properties object.
      /// Ex:
      /// {
      ///   "sample": { "data": 4 },
      ///   "sample2": { "data": { "nested": 5 } }
      /// }
      /// </summary>
      /// <param name="s"></param>
      //      public void Serialize(JsonSerializable.IStreamSerializer s)
      //      {
      //         var props = ContentSerialization.ConvertToDictionary(this);
      //         props.Serialize(s);
      //      }

      /// <summary>
      /// Set all the values of this content object from the given ArrayDict.
      /// This is a modifying action.
      /// </summary>
      /// <param name="dict">An array dict that should only contain the properties json object</param>
      //      public void ApplyProperties(ArrayDict dict)
      //      {
      //         ContentSerialization.ConvertFromDictionary(this, dict);
      //      }

      /// <summary>
      /// Create a new piece of content
      /// </summary>
      /// <param name="name">The name of the content, should be unique</param>
      /// <param name="dict">The property object to set the content values</param>
      /// <typeparam name="TContent">Some type of content</typeparam>
      /// <returns></returns>
      //      public static TContent FromDictionary<TContent>(string name, ArrayDict dict)
      //         where TContent : ContentObject, new()
      //      {
      //         var content = new TContent {ContentName = name};
      //         if (dict == null)
      //            return content;
      //
      //         content.ApplyProperties(dict);
      //         return content;
      //      }


      public static string GetContentTypeName(Type contentType)
      {
         return ContentRegistry.GetContentTypeName(contentType);
      }

      public static string GetContentType<TContent>()
         where TContent : ContentObject
      {
         return GetContentTypeName(typeof(TContent));
      }

      public static TContent Make<TContent>(string name)
         where TContent : ContentObject, new()
      {
         var instance = CreateInstance<TContent>();
         instance.SetContentName(name);
         return instance;
      }


      /// <summary>
      /// Validate this `ContentObject`.
      /// </summary>
      /// <exception cref="AggregateContentValidationException">Should throw if the content is semantically invalid.</exception>
      public virtual void Validate(IValidationContext ctx)
      {
         var errors = GetMemberValidationErrors(ctx);
         if (errors.Count > 0)
         {
            throw new AggregateContentValidationException(errors);
         }
      }

      #if UNITY_EDITOR


      public event Action<List<ContentException>> OnValidationChanged;
      public event Action OnEditorValidation;
      public static IValidationContext ValidationContext { get; set; }
      [IgnoreContentField]
      private bool _hadValidationErrors;
      public Guid ValidationGuid { get; set; }
      private void OnValidate()
      {
         ValidationGuid = Guid.NewGuid();
         OnEditorValidation?.Invoke();
         // access the edit time validation context?
         var ctx = ValidationContext ?? new ValidationContext();
         if (HasValidationExceptions(ctx, out var exceptions))
         {
            _hadValidationErrors = true;
            OnValidationChanged?.Invoke(exceptions);

         } else if (_hadValidationErrors)
         {
            _hadValidationErrors = false;
            OnValidationChanged?.Invoke(null);
         }
      }
      public void ForceValidate()
      {
         OnValidate();
      }
      #endif

      public bool HasValidationErrors(IValidationContext ctx, out List<string> errors)
      {
         errors = new List<string>();

         if (ContentName != null && ContentNameValidationException.HasNameValidationErrors(this, ContentName, out var nameErrors))
         {
            errors.AddRange(nameErrors.Select(e => e.Message));
         }

         errors.AddRange(GetMemberValidationErrors(ctx)
            .Select(e => e.Message));

         return errors.Count > 0;
      }

      public bool HasValidationExceptions(IValidationContext ctx, out List<ContentException> exceptions)
      {
         exceptions = new List<ContentException>();
         if (ContentName != null && ContentNameValidationException.HasNameValidationErrors(this, ContentName, out var nameErrors))
         {
            exceptions.AddRange(nameErrors);
         }
         exceptions.AddRange(GetMemberValidationErrors(ctx));
         return exceptions.Count > 0;
      }


      public List<ContentValidationException> GetMemberValidationErrors(IValidationContext ctx)
      {
         var errors = new List<ContentValidationException>();

         var seen = new HashSet<object>();
         var toExpand = new Queue<object>();

         toExpand.Enqueue(this);
         while (toExpand.Count > 0)
         {
            var obj = toExpand.Dequeue();
            if (seen.Contains(obj))
            {
               continue;
            }
            if (obj == null) continue;


            seen.Add(obj);
            var type = obj.GetType();

            if (type != typeof(string) && typeof(IEnumerable).IsAssignableFrom(type))
            {
               var set = (IEnumerable) obj;
               foreach (var subObj in set)
               {
                  toExpand.Enqueue(subObj);
               }
            }

            foreach (var field in type.GetFields())
            {
               var fieldValue = field.GetValue(obj);
               toExpand.Enqueue(fieldValue);

               foreach (var attribute in field.GetCustomAttributes<ValidationAttribute>())
               {
                  try
                  {
                     var wrapper = new ValidationFieldWrapper(field, obj);
                     attribute.Validate(wrapper, this, ctx);
                  }
                  catch (ContentValidationException e)
                  {
                     errors.Add(e);
                  }
               }

            }
         }

//         void Expand(object target, Type type)
//         {
//
//         }
//
//         foreach (var field in GetType().GetFields())
//         {
//            foreach (var attribute in field.GetCustomAttributes<ValidationAttribute>())
//            {
//               try
//               {
//                  attribute.Validate(field, this, ctx);
//               }
//               catch (ContentValidationException e)
//               {
//                  errors.Add(e);
//               }
//            }
//         }

         return errors;
      }

      public string ToJson()
      {
         return ClientContentSerializer.SerializeContent(this);
      }


   }
}