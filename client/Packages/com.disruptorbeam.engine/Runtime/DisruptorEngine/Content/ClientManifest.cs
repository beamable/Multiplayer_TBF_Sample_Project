using System;
using System.Collections.Generic;
using System.Linq;
using Beamable.Common.Content;

namespace Beamable.Content
{
   [System.Serializable]
   public class ClientManifest
   {
      public List<ClientContentInfo> entries;

      public static ClientManifest ParseCSV(string data)
      {
         // TODO: Consider replacing this with a more advanced csv parser... This method breaks many "rules"
         //       https://donatstudios.com/Falsehoods-Programmers-Believe-About-CSVs

         var lines = (data ?? "").Split('\n');

         var contentEntries = lines.Select(line =>
         {
            var parts = line.Split(new char[]{','}, StringSplitOptions.None);
            if (parts.Length <= 1)
            {
               return null; // skip line.
            }
            return new ClientContentInfo()
            {
               type = parts[0].Trim(),
               contentId = parts[1].Trim(),
               version = parts[2].Trim(),
               uri = parts[3].Trim(),
               tags = parts.Length >= 5
                  ? parts[4].Trim().Split(new []{';'}, StringSplitOptions.RemoveEmptyEntries)
                  : new string[]{}
            };
         }).Where(entry => entry != null);

         return new ClientManifest()
         {
            entries = contentEntries.ToList()
         };
      }
   }

   [System.Serializable]
   public class ClientContentInfo
   {
      public string contentId, version, uri, type;
      public string[] tags;

      public IContentRef AsReference()
      {
         var contentType = ContentRegistry.GetTypeFromId(contentId);
         return new ContentRef(contentType, contentId);
      }
   }
}