using System;
using System.Collections.Generic;
using System.IO;
using Beamable.Common;
using Beamable.Common.Api;
using Beamable.Common.Content;
using Beamable.Content.Serialization;
using Beamable.Api;
using Beamable.Spew;
using UnityEngine;

namespace Beamable.Content
{
   public abstract class ContentCache
   {
      public abstract Promise<IContentObject> GetContentObject(ClientContentInfo requestedInfo);
   }

   public class ContentCache<TContent> : ContentCache where TContent : ContentObject, new()
   {
      private readonly Dictionary<string, ContentCacheEntry<TContent>> cache = new Dictionary<string, ContentCacheEntry<TContent>>();
      private IBeamableRequester _requester;
      private static ClientContentSerializer _serializer = new ClientContentSerializer();

      public ContentCache(IBeamableRequester requester)
      {
         _requester = requester;
      }

      public override Promise<IContentObject> GetContentObject(ClientContentInfo requestedInfo)
      {
         return GetContent(requestedInfo).Map(content => (IContentObject)content);
      }

      public Promise<TContent> GetContent(ClientContentInfo requestedInfo)
      {
         var cacheId = requestedInfo.contentId;
         // First, try the in memory cache
         PlatformLogger.Log($"ContentCache: Fetching content from cache for {requestedInfo.contentId}: version: {requestedInfo.version}");
         if (cache.TryGetValue(cacheId, out var cacheEntry))
         {
            if (cacheEntry.Version == requestedInfo.version)
            {
               return cacheEntry.Content;
            }
         }

         // Then, try the on disk cache
         PlatformLogger.Log($"ContentCache: Loading content from disk for {requestedInfo.contentId}: version: {requestedInfo.version}");
         if (TryGetValueFromDisk(requestedInfo, out var diskContent))
         {
            cache.Add(cacheId, new ContentCacheEntry<TContent>(requestedInfo.version, diskContent));
            return diskContent;
         }

         // Finally, if not found, fetch the content from the CDN
         PlatformLogger.Log($"ContentCache: Fetching content from CDN for {requestedInfo.contentId}: version: {requestedInfo.version}");
         var fetchedContent = FetchContentFromCDN(requestedInfo)
            .Map(raw =>
            {
               // Write the content to disk
               SaveToDisk(requestedInfo, raw);
               return DeserializeContent(requestedInfo, raw);
            })
            .Error(err =>
            {
               cache.Remove(cacheId);
               PlatformLogger.Log($"ContentCache: Failed to resolve {requestedInfo.contentId} {requestedInfo.version} {requestedInfo.uri} ; ERR={err}");
            });
         cache.Add(cacheId, new ContentCacheEntry<TContent>(requestedInfo.version, fetchedContent));
         return fetchedContent;
      }

      private static bool TryGetValueFromDisk(ClientContentInfo info, out Promise<TContent> content)
      {
         var filePath = ContentPath(info);
         // Ensure the directory is created
         Directory.CreateDirectory(Path.GetDirectoryName(filePath));
         try
         {
            var raw = File.ReadAllText(filePath);
            var deserialized = DeserializeContent(info, raw);
            if (deserialized.Version == info.version)
            {
               content = Promise<TContent>.Successful(deserialized);
               return true;
            }

            content = null;
            return false;
         }
         catch (Exception e)
         {
            PlatformLogger.Log($"ContentCache: Error fetching content from disk: {e}");
            content = null;
            return false;
         }
      }

      private static void SaveToDisk(ClientContentInfo info, string raw)
      {
         try
         {
            var filePath = ContentPath(info);
            // Ensure the directory is created
            Directory.CreateDirectory(Path.GetDirectoryName(filePath));
            File.WriteAllText(filePath, raw);
         }
         catch (Exception e)
         {
            PlatformLogger.Log($"ContentCache: Error saving content to disk: {e}");
         }
      }

      private static string ContentPath(ClientContentInfo info)
      {
         return Application.persistentDataPath + $"/content/{info.contentId}.json";
      }

      private Promise<string> FetchContentFromCDN(ClientContentInfo info)
      {
         return _requester.Request(Method.GET, info.uri, includeAuthHeader: false, parser: s => s);
      }

      private static TContent DeserializeContent(ClientContentInfo info, string raw)
      {
         return _serializer.Deserialize<TContent>(raw);

//         var rawDict = Json.Deserialize(raw) as ArrayDict;
//         var contentName = string.Join(".", info.contentId.Split('.').Skip(1));
//         var createdContent = ContentObject.Make<TContent>(contentName);
//         createdContent.ApplyProperties(rawDict?["properties"] as ArrayDict);
//         createdContent.SetContentMetadata(contentName, rawDict?["version"] as string);
//         return createdContent;
      }
   }

   public struct ContentCacheEntry<TContent> where TContent : ContentObject
   {
      public readonly string Version;
      public readonly Promise<TContent> Content;

      public ContentCacheEntry(string version, Promise<TContent> content)
      {
         Version = version;
         Content = content;
      }
   }
}