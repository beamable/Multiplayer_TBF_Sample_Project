using System;
using System.Collections.Generic;
using Beamable.Common;
using Beamable.Common.Api;

namespace Beamable.Api.Matchmaking
{
    public class MatchmakingService
    {
        private PlatformRequester _requester;
        public MatchmakingService (PlatformRequester requester)
        {
            _requester = requester;
        }

        /// <summary>
        /// Find this player a match for the given game type
        /// </summary>
        /// <param name="gameType">The string id of the game type we wish to be matched</param>
        /// <returns></returns>
        public Promise<MatchmakingResponse> Match(string gameType) {
            return _requester.Request<MatchmakingResponse>(
                Method.POST,
                $"/object/matchmaking/{gameType}/match"
            );
        }
    }

    [Serializable]
    public class MatchmakingResponse
    {
        public string game;
        public int ticksRemaining;
        public List<long> players;
    }
}