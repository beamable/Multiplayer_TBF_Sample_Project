using Beamable.Common;
using Beamable.Common.Content;



namespace Beamable.Content
{
    [ContentType("leaderboards")]
    [System.Serializable]
    [Agnostic]
    public class LeaderboardContent : ContentObject
    {
        public ClientPermissions permissions;
    }

    [System.Serializable]
    [Agnostic]
    public class LeaderboardRef : ContentRef<LeaderboardContent> {}
}