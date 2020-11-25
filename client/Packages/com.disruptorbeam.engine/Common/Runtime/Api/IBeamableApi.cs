namespace Beamable.Common.Api
{
   public interface IBeamableApi
   {
      IBeamableRequester Requester { get; }
   }
}