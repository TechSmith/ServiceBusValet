using System.Collections.Generic;
using Microsoft.ServiceBus;
using Microsoft.ServiceBus.Messaging;
using NLog;

namespace TechSmith.ServiceBusValet.Services
{
   public class TopicService
   {
      private static readonly Logger _logger = LogManager.GetLogger( "TopicService" );

      private readonly string _connectionString;
      private readonly string _topicName;

      public TopicService( string connectionString, string topicName )
      {
         _connectionString = connectionString;
         _topicName = topicName;
      }

      public TopicDescription CreateTopicIfNotExists()
      {
         NamespaceManager namespaceManager = NamespaceManager.CreateFromConnectionString( _connectionString );

         try
         {
            if ( namespaceManager.TopicExists( _topicName ) )
            {
               _logger.Info( "Topic '{0}' already exists", _topicName );
               return namespaceManager.GetTopic( _topicName );
            }
            _logger.Info( "Creating new Topic: '{0}'", _topicName );  
            return namespaceManager.CreateTopic( _topicName );
         }
         catch ( MessagingException ex )
         {
            _logger.Warn( "Exception while creating topic", ex );
            throw;
         }
      }

      public void SendMessage( BrokeredMessage brokeredMessage )
      {
         var topicClient = TopicClient.CreateFromConnectionString( _connectionString, _topicName );

         try
         {
            topicClient.Send( brokeredMessage );
         }
         catch ( MessagingException ex )
         {
            if ( !ex.IsTransient )
            {
               _logger.Warn( "Exception while sending message", ex );
               throw;
            }
            _logger.Warn( "Transient exception while sending message", ex );
         }
      }

      public IEnumerable<SubscriptionDescription> GetSubscriptions()
      {
         NamespaceManager namespaceManager = NamespaceManager.CreateFromConnectionString( _connectionString );
         return namespaceManager.GetSubscriptions( _topicName );
      }
   }
}
