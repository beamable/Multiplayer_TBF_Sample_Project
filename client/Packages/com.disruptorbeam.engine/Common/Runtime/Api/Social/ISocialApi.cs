namespace Beamable.Common.Api.Social
{
   public interface ISocialApi
   {
      Promise<SocialList> Get();
      Promise<PlayerBlockStatus> BlockPlayer(long gamerTag);
      Promise<PlayerBlockStatus> UnblockPlayer(long gamerTag);
      Promise<EmptyResponse> SendFriendRequest (long gamerTag);
      Promise<EmptyResponse> RemoveFriend (long gamerTag);
      Promise<SocialList> RefreshSocialList();
   }
}