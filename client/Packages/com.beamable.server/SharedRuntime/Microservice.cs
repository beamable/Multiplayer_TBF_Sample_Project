using System;
using Beamable.Common;
using Beamable.Common.Api;

namespace Beamable.Server
{
   public delegate TMicroService ServiceFactory<out TMicroService>() where TMicroService : Microservice;

   public abstract class Microservice
   {
      protected RequestContext Context;
      protected IBeamableRequester Requester;
      protected IBeamableServices Services;

      public void ProvideContext(RequestContext ctx)
      {
         Context = ctx;
      }

      public void ProvideRequester(IBeamableRequester requester)
      {
         Requester = requester;
      }

      public void ProvideServices(IBeamableServices services)
      {
         Services = services;
      }
   }
}
