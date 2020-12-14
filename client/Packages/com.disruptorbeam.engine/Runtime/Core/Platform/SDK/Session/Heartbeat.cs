using UnityEngine;
using System.Collections;
using Beamable.Coroutines;

namespace Beamable.Api.Sessions
{
   public class Heartbeat
   {

      private readonly PlatformService _platform;
      private readonly CoroutineService _coroutineService;
      private readonly WaitForSeconds _wait;

      public Heartbeat(PlatformService platform, CoroutineService coroutineService, int secondsInterval)
      {
         _wait = Yielders.Seconds(secondsInterval);
         _platform = platform;
         _coroutineService = coroutineService;
      }

      public void Start()
      {
         ScheduleHeartbeat();
      }

      private void ScheduleHeartbeat()
      {
         _coroutineService.StartCoroutine(SendHeartbeat());
      }

      /// <summary>
      /// Coroutine: send heartbeat requests to Platform.
      /// </summary>
      private IEnumerator SendHeartbeat()
      {
         yield return _wait;
         if (_platform.ConnectivityService.HasConnectivity)
         {
            _platform.Session.SendHeartbeat()
            .Then(rsp => ScheduleHeartbeat())
            .Error(err => ScheduleHeartbeat());
         }
         else
         {
            ScheduleHeartbeat();
         }
      }
   }
}