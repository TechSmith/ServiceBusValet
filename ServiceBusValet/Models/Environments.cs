using System.Collections.Generic;
using System.IO;
using System.Linq;
using CsvHelper;

namespace TechSmith.ServiceBusValet.Models
{
   public class ServiceBusEnvironments
   {
      private readonly Dictionary<string, string> _connectionStrings;

      public ServiceBusEnvironments( string path )
      {
         var csvReader = new CsvReader( new StreamReader( path ) );
         _connectionStrings = csvReader.GetRecords<EnvironmentConnectionString>().ToDictionary( e => e.Environment, e => e.ConnectionString );
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

      private class EnvironmentConnectionString
      {
         public string Environment
         {
            get;
            set;
         }
         public string ConnectionString
         {
            get;
            set;
         }
      }
   }
}
