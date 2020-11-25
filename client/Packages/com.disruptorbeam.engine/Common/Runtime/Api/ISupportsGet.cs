namespace Beamable.Common.Api
{
   public interface ISupportsGet<TData>
   {
      Promise<TData> GetCurrent(string scope="");
   }

   public interface ISupportGetLatest<out TData>
   {
      TData GetLatest(string scope = "");
   }

   public class BeamableGetApiResource<ScopedRsp>
   {
      public Promise<ScopedRsp> RequestData(IBeamableRequester requester, IUserContext ctx, string serviceName, string scope)
      {
         return RequestData(requester, CreateRefreshUrl(ctx, serviceName, scope));
      }

      public Promise<ScopedRsp> RequestData(IBeamableRequester requester, string url)
      {
         return requester.Request<ScopedRsp>(Method.GET, url);
      }

      public string CreateRefreshUrl(IUserContext ctx, string serviceName, string scope)
      {
         var queryArgs = "";
         if (!string.IsNullOrEmpty(scope))
         {
            queryArgs = $"?scope={scope}";
         }

         return $"/object/{serviceName}/{ctx.UserId}{queryArgs}";
      }
   }
}