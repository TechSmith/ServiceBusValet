using System.Collections.Generic;
using Microsoft.ServiceBus;
using Microsoft.ServiceBus.Messaging;

namespace TechSmith.ServiceBusValet.Services
{
   public class ConnectionService
   {
      private readonly string _connectionString;

      public ConnectionService( string connectionString )
      {
         _connectionString = connectionString;
      }

      public IEnumerable<TopicDescription> GetTopics()
      {
         NamespaceManager namespaceManager = NamespaceManager.CreateFromConnectionString( _connectionString );
         return namespaceManager.GetTopics();
      }
   }
}
