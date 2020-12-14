using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace Beamable.Api.Sim {
   /**
   *	A SimNetworkInterface guarantees that a given frame will only be sent to peers a maximum of 1 time and that a given frame will be surfaced to the SimClient a maximum of 1 time
   */
   public interface SimNetworkInterface {

      // Get a unique id for the client which is consistent across the network
      string ClientId { get; }

      // Is the network ready to operate?
      bool Ready { get; }

      // Synchronize the network interface and receive any fully realized frames by the network
      List<SimFrame> Tick (long curFrame, long maxFrame, long expectedMaxFrame);

      // Push (or queue) an event onto the network
      void SendEvent (SimEvent evt);

   }
}