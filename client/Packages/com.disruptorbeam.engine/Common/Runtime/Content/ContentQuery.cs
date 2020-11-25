using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Beamable.Common.Content;

namespace Beamable.Editor.Content
{
   public static class ContentQueryParser
   {
      public delegate void ApplyParseRule<in T>(string raw, T query) where T : ContentQuery;

      public delegate bool SerializeRule<in T>(T query, out string serializedpart) where T : ContentQuery;

      public static string ToString<T>(string existing, T query, List<SerializeRule<T>> serializeRules, Dictionary<string, ApplyParseRule<T>> parseRules)
         where T : ContentQuery, new()
      {
         if (existing == null)
         {
            existing = "";
         }
         else
         {
            var standardParse = Parse<T>(existing, parseRules);

            if (standardParse.Equals(query))
            {
               return existing; // easy way out.
            }
         }

         var additionalParts = new List<string>();
         var partMap = new Dictionary<string, string>();

         foreach (var rule in serializeRules)
         {
            if (!rule.Invoke(query, out var clause)) continue;

            if (clause.Contains(":"))
            {
               var index = clause.IndexOf(':');
               var leftPart = clause.Substring(0, index);
               var rightPart = clause.Substring(index + 1);
               partMap[leftPart] = rightPart;
            }
            else if (!string.IsNullOrEmpty(clause))
            {
               partMap["id"] = clause;
            }
            if (!existing.Contains(clause))
            {
               additionalParts.Add(clause);
            }
         }

         var strParts = new List<string>();
         void HandleCouple(string leftPart, string rightPart)
         {

            if (string.IsNullOrEmpty(leftPart))
            {
               leftPart = "id";
            }

            var leftText = (leftPart.Equals("id") ? "" : $"{leftPart}:");

            if (partMap.TryGetValue(leftPart.Trim(), out var existingRightPart))
            {
               partMap.Remove(leftPart.Trim());

               strParts.Add($"{leftText}{existingRightPart}");
            }
            else if (!string.IsNullOrEmpty(leftPart))
            {
               strParts.Add($"{leftText}{rightPart}");
            }
            else
            {
               strParts.Add(rightPart);
            }
         }

         var buffer = "";
         var left = "";
         var right = "";

         for (var i = 0; i < existing.Length; i++)
         {
            var c = existing[i];
            switch (c)
            {
               case ':':
                  left = buffer;
                  buffer = "";
                  break;
               case ',':
                  // parse the buffer for a couple grouping.
                  right = buffer;
                  buffer = "";
                  HandleCouple(left, right);
                  left = "";
                  right = "";
                  break;
               default:
                  buffer += c;
                  break;
            }
         }

         right = buffer;
         if (!string.IsNullOrEmpty(left) || !string.IsNullOrEmpty(right))
         {
            HandleCouple(left, right);
         }


         var extraPartStr = "";
         if (partMap.Count > 0)
         {
            var partStr = partMap.Select(kvp => kvp.Key.Equals("id")
               ? kvp.Value
               : $"{kvp.Key}:{kvp.Value}").ToList();

            extraPartStr = string.Join(", ", partStr);
            //strParts.AddRange(partStr);
         }
         var strOut = string.Join(",", strParts);
         if (strOut.EndsWith(",") || strOut.Length == 0)
         {
            strOut += extraPartStr;
         }
         else if (extraPartStr.Length > 0)
         {
            strOut += $", {extraPartStr}";
         }



         return strOut;
      }

      public static T Parse<T>(string raw, Dictionary<string, ApplyParseRule<T>> rules)
         where T: ContentQuery, new()
      {
         var output = new T();
         if (string.IsNullOrEmpty(raw))
            return output;

         void ParseCouple(string leftPart, string rightPart) // tag: hello foo; id:tuna frank:man
         {
            leftPart = leftPart.Trim();
            rightPart = rightPart.Trim();

            if (string.IsNullOrEmpty(leftPart))
            {
               ApplyIdParse(rightPart, output);
            } else if (rules.TryGetValue(leftPart, out var rule))
            {
               rule?.Invoke(rightPart, output);
            }
            else
            { // ???
            }
         }

         var buffer = "";
         var left = "";
         var right = "";

         for (var i = 0; i < raw.Length; i++)
         {
            var c = raw[i];
            switch (c)
            {
               case ':':
                  left = buffer;
                  buffer = "";
                  break;
               case ',':
                  // parse the buffer for a couple grouping.
                  right = buffer;
                  buffer = "";
                  ParseCouple(left, right);
                  left = "";
                  right = "";
                  break;
               default:
                  buffer += c;
                  break;
            }
         }

         right = buffer;
         if (!string.IsNullOrEmpty(left) || !string.IsNullOrEmpty(right))
         {
            ParseCouple(left, right);
         }
         return output;
      }

      public static void ApplyIdParse(string raw, ContentQuery query)
      {
         query.IdContainsConstraint = raw;
      }

   }

   public class ContentQuery
   {
      public static readonly ContentQuery Unit = new ContentQuery();

      public HashSet<Type> TypeConstraints;
      public HashSet<string> TagConstraints;
      public string IdContainsConstraint;

      public ContentQuery()
      {

      }

      public ContentQuery(ContentQuery other)
      {
         if (other == null) return;

         TypeConstraints = other.TypeConstraints != null
            ? new HashSet<Type>(other.TypeConstraints.ToArray())
            : null;
         TagConstraints = other.TagConstraints != null
            ? new HashSet<string>(other.TagConstraints.ToArray())
            : null;
         IdContainsConstraint = other.IdContainsConstraint;
      }

      protected static void ApplyTypeParse(string raw, ContentQuery query)
      {
         try
         {
            var typeNames = raw.Split(new[] {' '}, StringSplitOptions.RemoveEmptyEntries);

            var types = new HashSet<Type>();
            foreach (var typeName in typeNames)
            {
               try
               {
                  var type = ContentRegistry.NameToType(typeName);
                  types.Add(type);
               }
               catch (Exception ex)
               {

               }
            }
            query.TypeConstraints = new HashSet<Type>(types);

         }
         catch (Exception)
         {
            // don't do anything.
            //query.TypeConstraint = typeof(int); // something to block filtering from working.
         }
      }


      protected static void ApplyTagParse(string raw, ContentQuery query)
      {
         var tags = raw.Split(new[] {' '}, StringSplitOptions.RemoveEmptyEntries);
         query.TagConstraints = new HashSet<string>(tags);
      }

      protected static readonly Dictionary<string, ContentQueryParser.ApplyParseRule<ContentQuery>> StandardRules = new Dictionary<string, ContentQueryParser.ApplyParseRule<ContentQuery>>
      {
         {"t", ApplyTypeParse},
         {"id", ContentQueryParser.ApplyIdParse},
         {"tag", ApplyTagParse},
      };

      protected static readonly List<ContentQueryParser.SerializeRule<ContentQuery>> StandardSerializeRules = new List<ContentQueryParser.SerializeRule<ContentQuery>>
      {
         SerializeTagRule, SerializeTypeRule, SerializeIdRule
      };

      public static ContentQuery Parse(string text)
      {
         return ContentQueryParser.Parse(text, StandardRules);
      }

      public virtual bool Accept(IContentObject content)
      {
         return AcceptTag(content) && AcceptIdContains(content) && AcceptType(content.GetType());
      }

      public bool AcceptTag(IContentObject content)
      {
         if (TagConstraints == null) return true;
         if (content == null)
         {
            return TagConstraints.Count == 0;
         }

         return AcceptTags(new HashSet<string>(content.Tags));
      }

      public bool AcceptTags(HashSet<string> tags)
      {
         if (TagConstraints == null) return true;
         if (tags == null) return TagConstraints.Count == 0;

         foreach (var tag in TagConstraints)
         {
            if (!tags.Contains(tag))
            {
               return false;
            }
         }

         return true;
      }

      public bool AcceptType<TContent>(bool allowInherit=true) where TContent : IContentObject, new()
      {
         return AcceptType(typeof(TContent), allowInherit);
      }

      public bool AcceptType(Type type, bool allowInherit=true)
      {
         if (TypeConstraints == null || TypeConstraints.Count == 0) return true;

         if (type == null) return false;

         if (allowInherit)
         {
            return TypeConstraints.Any(t => t.IsAssignableFrom(type));
         }
         else
         {
            return TypeConstraints.Contains(type);
         }
      }

      public bool AcceptIdContains(IContentObject content)
      {
         return AcceptIdContains(content?.Id);
      }

      public bool AcceptIdContains(string id)
      {
         if (IdContainsConstraint == null) return true;
         if (id == null) return false;
         return id.Split('.').Last().Contains(IdContainsConstraint);
      }

      protected static bool SerializeTagRule(ContentQuery query, out string str)
      {
         str = "";
         if (query.TagConstraints == null)
         {
            return false;
         }
         str = $"tag:{string.Join(" ", query.TagConstraints)}";
         return true;
      }

      protected static bool SerializeTypeRule(ContentQuery query, out string str)
      {
         str = "";
         if (query.TypeConstraints == null)
         {
            return false;
         }
         str = $"t:{string.Join(" ", query.TypeConstraints.Select(ContentRegistry.TypeToName))}";
         return true;
      }

      protected static bool SerializeIdRule(ContentQuery query, out string str)
      {
         str = "";
         if (query.IdContainsConstraint == null)
         {
            return false;
         }
         str = $"{query.IdContainsConstraint}";
         return true;
         }

      public bool EqualsContentQuery(ContentQuery other)
      {
         if (other == null) return false;

         var tagsEqual = other.TagConstraints == null || TagConstraints == null
            ? (other.TagConstraints == null && TagConstraints == null)
            : (other.TagConstraints.SetEquals(TagConstraints));

         var typesEqual = other.TypeConstraints == null || TypeConstraints == null
            ? (other.TypeConstraints == null && TypeConstraints == null)
            : other.TypeConstraints.SetEquals(TypeConstraints);

         var idEqual = (other.IdContainsConstraint?.Equals(IdContainsConstraint) ?? IdContainsConstraint == null);
         return tagsEqual &&
                 idEqual &&
                 typesEqual;
      }

      public override bool Equals(object obj)
      {
         return EqualsContentQuery(obj as ContentQuery);
      }

      public override int GetHashCode()
      {
         unchecked
         {
            var hashCode = (TypeConstraints != null ? TypeConstraints.GetHashCode() : 0);
            hashCode = (hashCode * 397) ^ (TagConstraints != null ? TagConstraints.GetHashCode() : 0);
            hashCode = (hashCode * 397) ^ (IdContainsConstraint != null ? IdContainsConstraint.GetHashCode() : 0);
            return hashCode;
         }
      }

      public string ToString(string existing)
      {
         return ContentQueryParser.ToString(existing, this, StandardSerializeRules, StandardRules);
      }

      public override string ToString()
      {
         return ToString(null);
      }
   }
}