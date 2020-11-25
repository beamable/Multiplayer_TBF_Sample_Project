using UnityEngine;
using System;
using System.Collections.Generic;
using Beamable.Common;

namespace Beamable.Platform.SDK.Sessions
{
   public class SessionService
   {
      private static long TTL_MS = 60 * 1000;

      private UserDataCache<Session> cache;
      private PlatformService _platform;
      private PlatformRequester _requester;

      public SessionService (PlatformService platform, PlatformRequester requester)
      {
         _platform = platform;
         _requester = requester;
         cache = new UserDataCache<Session>("Session", TTL_MS, resolve);
      }

      private Promise<Dictionary<long, Session>> resolve(List<long> gamerTags)
      {
         string queryString = "";
         for (int i = 0; i < gamerTags.Count; i++)
         {
            if (i > 0)
            {
               queryString += "&";
            }
            queryString += String.Format("gts={0}", gamerTags[i]);
         }

         return _requester.Request<MultiOnlineStatusesResponse>(
            Method.GET,
            String.Format("/presence/bulk?{0}", queryString)
         ).Map(rsp =>
         {
            Dictionary<long, Session> result = new Dictionary<long, Session>();
            var dict = rsp.ToDictionary();
            for (int i = 0; i < gamerTags.Count; i++)
            {
               if (!dict.ContainsKey(gamerTags[i]))
               {
                  dict[gamerTags[i]] = 0;
               }
               result.Add(gamerTags[i], new Session(dict[gamerTags[i]]));
            }
            return result;
         });
      }

      public Promise<EmptyResponse> StartSession (string advertisingId, string locale) {
         return _requester.Request<EmptyResponse>(
            Method.POST,
            "/basic/session",
            new SessionStartRequest(advertisingId, locale)
         );
      }

      public Promise<EmptyResponse> SendHeartbeat() {
         return _requester.Request<EmptyResponse>(
            Method.POST,
            "/basic/session/heartbeat"
         );
      }

      public Promise<Session> GetHeartbeat (long gamerTag)
      {
         return cache.Get(gamerTag);
      }
   }

   [Serializable]
   class SessionStartRequest
   {
      public string platform;
      public string device;
      public string locale;
      public DeviceParams deviceParams;

      public SessionStartRequest (string advertisingId, string locale)
      {
         this.platform = Application.platform.ToString();
         this.device = SystemInfo.deviceModel.ToString();
         this.locale = locale;
         this.deviceParams = new DeviceParams(advertisingId);
      }
   }

   [Serializable]
   class DeviceParams
   {
      public string device;
      public string osversion;
      public string device_mem_onboard;
      public string gfx_device_id;
      public string gfx_device_name;
      public string gfx_vendor;
      public string gfx_vendor_id;
      public string gfx_version;
      public string gfx_memory;
      public string gfx_shader_level;
      public string cpu_processor_count;
      public string cpu_processor_type;
      public string ios_device_generation;
      public string ios_system_version;
      public string idfa;
      public string gaid;

      public DeviceParams (string advertisingId)
      {
         this.device = SystemInfo.deviceModel.ToString();
#if USE_STEAMWORKS
			this.osversion = "Steam";
#else
			this.osversion = SystemInfo.operatingSystem.ToString();
#endif
         this.device_mem_onboard = SystemInfo.systemMemorySize.ToString();
         this.gfx_device_id = SystemInfo.graphicsDeviceID.ToString();
         this.gfx_device_name = SystemInfo.graphicsDeviceName;
         this.gfx_vendor = SystemInfo.graphicsDeviceVendor;
         this.gfx_vendor_id = SystemInfo.graphicsDeviceVendorID.ToString();
         this.gfx_version = SystemInfo.graphicsDeviceVersion.ToString();
         this.gfx_memory = SystemInfo.graphicsMemorySize.ToString();
         this.gfx_shader_level = SystemInfo.graphicsShaderLevel.ToString();
         this.cpu_processor_count = SystemInfo.processorCount.ToString();
			this.cpu_processor_type = SystemInfo.processorType.ToString();

#if UNITY_IOS
         this.idfa = advertisingId;
         this.ios_device_generation = UnityEngine.iOS.Device.generation.ToString();
         this.ios_system_version = UnityEngine.iOS.Device.systemVersion.ToString();
#else
         this.idfa = "";
         this.ios_device_generation = "";
         this.ios_system_version = "";
#endif
#if UNITY_ANDROID
         this.gaid = advertisingId;
#else
         this.gaid = "";
#endif
      }
   }

   [Serializable]
   public class MultiOnlineStatusesResponse
   {
      public List<SessionHeartbeat> statuses;

      public Dictionary<long, long> ToDictionary () {
         Dictionary<long, long> result = new Dictionary<long, long>();
         for (int i=0; i<statuses.Count; i++) {
            var next = statuses[i];
            result[next.gt] = next.heartbeat;
         }
         return result;
      }
   }

   [Serializable]
   public class SessionHeartbeat
   {
      public long gt;
      public long heartbeat;
   }

   public class Session
   {
      private static readonly DateTime Jan1st1970 = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
      private static long CurrentTimeSeconds() {
         return (long) (DateTime.UtcNow - Jan1st1970).TotalSeconds;
      }

      public long Heartbeat;
      public long LastSeenMinutes;

      public Session(long heartbeat)
      {
         Heartbeat = heartbeat;
         LastSeenMinutes = (CurrentTimeSeconds() - heartbeat) / 60;
      }
   }
}