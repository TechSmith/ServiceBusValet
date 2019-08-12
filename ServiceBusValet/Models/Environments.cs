using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.Text;

namespace TechSmith.ServiceBusValet.Models
{
   public class ServiceBusEnvironments
   {
      private readonly Dictionary<string, string> _connectionStrings;

      public ServiceBusEnvironments( string path )
      {
         string configurationJson = File.ReadAllText( path );
         MemoryStream memoryStream = new MemoryStream( Encoding.UTF8.GetBytes( configurationJson ) );
         DataContractJsonSerializer serializer = new DataContractJsonSerializer( typeof( Environments ) );
         Environments environments = serializer.ReadObject( memoryStream ) as Environments;
         memoryStream.Close();
         _connectionStrings = environments.EnvironmentConnectionStrings.ToDictionary( e => e.Environment, e => e.ConnectionString );
      }

      public IEnumerable<string> GetNames()
      {
         foreach( var connectionString in _connectionStrings )
         {
            yield return connectionString.Key;
         }
      }

      public string GetConnectionString( string environmentName )
      {
         return _connectionStrings[environmentName];
      }

      [DataContract]
      private class Environments
      {
         [DataMember]
         public List<EnvironmentConnectionString> EnvironmentConnectionStrings
         {
            get;
            set;
         }
      }

      [DataContract]
      private class EnvironmentConnectionString
      {
         [DataMember]
         public string Environment
         {
            get;
            set;
         }

         [DataMember]
         public string ConnectionString
         {
            get;
            set;
         }
      }
   }
}
