namespace Beamable.Server
{
   [System.AttributeUsage(System.AttributeTargets.Method)]
   public class ClientCallableAttribute : System.Attribute
   {
      private string pathName = "";

      public ClientCallableAttribute()
      {

      }

      public ClientCallableAttribute(string pathnameOverride)
      {
         pathName = pathnameOverride;
      }

      public string PathName
      {
         set { pathName = value; }
         get { return pathName; }
      }
   }
}