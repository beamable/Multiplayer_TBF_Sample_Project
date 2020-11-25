using System;
using System.Collections.Generic;
using System.Linq;
using Beamable.Common.Content;
using Beamable.Editor.Content.SaveRequest;
using Beamable.Serialization;

namespace Beamable.Editor.Content
{

   public class Manifest
   {
      private Dictionary<string, ContentManifestReference> _lookup = new Dictionary<string, ContentManifestReference>();
      private ContentManifest _source;
      private IEnumerable<ContentManifestReference> _allReferences;

      public List<ManifestReferenceSuperset> References => _allReferences
         .Select(r => new ManifestReferenceSuperset()
         {
            Checksum = r.checksum,
            Id = r.id,
            Type = r.type,
            Uri = r.uri,
            Visibility = r.visibility,
            Version = r.version,
            Tags = r.tags
         })
         .ToList(); // shallow copy.

      public Manifest(IEnumerable<ContentManifestReference> references)
      {
         _lookup = references.ToDictionary(r => r.id);
         _allReferences = references;
      }

      public Manifest(ContentManifest source)
      {
         _source = source;
         _allReferences = source.references;
         _lookup = source.references
            .Where(r => r.visibility.Equals("public"))
            .ToDictionary(r => r.id);
      }

      public ContentManifestReference Get(string id)
      {
         _lookup.TryGetValue(id, out ContentManifestReference result);
         return result;
      }

      public static ManifestDifference FindDifferences(Manifest current, Manifest next)
      {
         // a change set between manifests includes MODIFICATIONS, ADDITIONS, and DELETIONS

         var currentIds = current._lookup.Keys;

         var unseenIds = new HashSet<string>();
         next._lookup.Keys.ToList().ForEach(id => unseenIds.Add(id));

         var additions = new List<ContentManifestReference>();
         var modifications = new List<ContentManifestReference>();

         foreach (var id in currentIds)
         {
            // to facilitate deletions, take note of each id we've seen, so that at the end of iteration, the set only contains ids not existing in currentIds
            unseenIds.Remove(id);

            var nextContent = next.Get(id);
            var currentContent = current.Get(id);

            if (nextContent == null)
            {
               // only exists in current. counts as an addition to the next set.
               additions.Add(currentContent);
               continue;
            }

            if (!nextContent.checksum.Equals(currentContent.checksum) || !nextContent.tags.SequenceEqual(currentContent.tags))
            {
               modifications.Add(currentContent);
            }
         }

         var deletions = unseenIds.Select(id => next.Get(id)).ToList();

         return new ManifestDifference()
         {
            Additions = additions,
            Modifications = modifications,
            Deletions = deletions
         };
      }
   }

   public class ManifestDifference
   {
      public IEnumerable<ContentManifestReference> Additions, Deletions, Modifications;
   }

   [System.Serializable]
   public class ContentManifest : JsonSerializable.ISerializable
   {
      public string id;
      public long created;
      public List<ContentManifestReference> references;

      public void Serialize(JsonSerializable.IStreamSerializer s)
      {
         s.Serialize(nameof(id), ref id);
         s.Serialize(nameof(created), ref created);
         s.SerializeList(nameof(references), ref references);
      }
   }

   [System.Serializable]
   public class ContentManifestReference : JsonSerializable.ISerializable
   {
      public string id;
      public string version;
      public string type;
      public string[] tags;
      public string uri;
      public string checksum;
      public string visibility;

      public void Serialize(JsonSerializable.IStreamSerializer s)
      {
         s.Serialize(nameof(id), ref id);
         s.Serialize(nameof(version), ref version);
         s.Serialize(nameof(type), ref type);
         s.SerializeArray(nameof(tags), ref tags);
         s.Serialize(nameof(uri), ref uri);
         s.Serialize(nameof(checksum), ref checksum);
         s.Serialize(nameof(visibility), ref visibility);
      }
   }

   [System.Serializable]
   public class LocalContentManifest
   {
      public Dictionary<string, LocalContentManifestEntry> Content = new Dictionary<string, LocalContentManifestEntry>();
   }

   [System.Serializable]
   public class LocalContentManifestEntry
   {
      public Type ContentType;
      public string Id => Content.Id;
      public string TypeName => Content.Id.Substring(0, Content.Id.LastIndexOf('.'));
      public string[] Tags => Content.Tags;
      public string Version => Content.Version;
      public string AssetPath;
      public IContentObject Content;

   }
}