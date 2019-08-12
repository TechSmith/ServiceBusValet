using System;
using System.Collections.Generic;
using System.ComponentModel;
using Microsoft.ServiceBus;
using Microsoft.ServiceBus.Messaging;
using NLog;

namespace TechSmith.ServiceBusValet.Services
{
   public class SubscriptionService
   {
      private static readonly Logger _logger = LogManager.GetLogger( "SubscriptionService" );  

      private readonly string _connectionString;
      private readonly string _topicName;
      private readonly string _subscriptionName;

      private BackgroundWorker _backgroundWorker;

      public SubscriptionService( string connectionString, string topicName, string subscriptionName )
      {
         _connectionString = connectionString;
         _topicName = topicName;
         _subscriptionName = subscriptionName;
      }

      public SubscriptionDescription CreateSubscriptionIfNotExists()
      {
         NamespaceManager namespaceManager = NamespaceManager.CreateFromConnectionString( _connectionString );

         if ( namespaceManager.SubscriptionExists( _topicName, _subscriptionName ) )
         {
            _logger.Info( "Subscription: '{0}' already exists for Topic: '{1}'", _subscriptionName, _topicName );

            return namespaceManager.GetSubscription( _topicName, _subscriptionName );
         }
         _logger.Info( "Creating new Subscription: '{0}' for Topic: '{1}'", _subscriptionName, _topicName );  
         return namespaceManager.CreateSubscription( _topicName, _subscriptionName );
      }

      public long CountMessages()
      {
         NamespaceManager namespaceManager = NamespaceManager.CreateFromConnectionString( _connectionString );
         var subscriptionDescription = namespaceManager.GetSubscription( _topicName, _subscriptionName );
         return subscriptionDescription.MessageCount;
      }

      public MessageCountDetails GetMessageCountDetails()
      {
         NamespaceManager namespaceManager = NamespaceManager.CreateFromConnectionString( _connectionString );
         var subscriptionDescription = namespaceManager.GetSubscription( _topicName, _subscriptionName );
         return subscriptionDescription.MessageCountDetails;
      }

      private bool IsBackgroundOperationInProgress()
      {
         if ( _backgroundWorker != null && _backgroundWorker.IsBusy )
         {
            _logger.Warn( "Background operation currently in progress. Please wait for it to complete or stop it before executing another job" );
            return true;
         }
         return false;
      }

      public void ResendDeadletterMessages( int messageCount )
      {
         if ( IsBackgroundOperationInProgress() )
         {
            return;
         }
         _backgroundWorker = new BackgroundWorker();
         _backgroundWorker.WorkerSupportsCancellation = true;
         _backgroundWorker.DoWork += ResendDeadletterMessages_DoWork;
         _backgroundWorker.RunWorkerAsync( messageCount );
      }

      private void ResendDeadletterMessages_DoWork( object sender, DoWorkEventArgs e )
      {
         int messageCount = (int) e.Argument;
         MessagingFactory messagingFactory = MessagingFactory.CreateFromConnectionString( _connectionString );
         var deadLetterPath = SubscriptionClient.FormatDeadLetterPath( _topicName, _subscriptionName );
         MessageReceiver messageReceiver = messagingFactory.CreateMessageReceiver( deadLetterPath );
         var topicService = new TopicService( _connectionString, _topicName );
         for ( int i = 0; i < messageCount; i++ )
         {
            if ( _backgroundWorker.CancellationPending )
            {
               return;
            }
            try
            {
               BrokeredMessage message = messageReceiver.Receive( TimeSpan.FromSeconds( 5 ) );
               if ( message != null )
               {
                  _logger.Info( "{0} - Moving message '{1}' to active queue", i + 1, message.MessageId );
                  topicService.SendMessage( message.Clone() );
                  message.Complete();
               }
               else
               {
                  // No more messages to receive so break out
                  break;
               }
            }
            catch ( MessagingException ex )
            {
               if ( !ex.IsTransient )
               {
                  _logger.Warn( "Exception while resending deadletter message", ex );
                  throw;
               }
               _logger.Warn( "Transient exception while resending deadletter message", ex );
            }
         }
      }

      public void DeadletterMessages( int messageCount )
      {
         if ( IsBackgroundOperationInProgress() )
         {
            return;
         }
         _backgroundWorker = new BackgroundWorker();
         _backgroundWorker.WorkerSupportsCancellation = true;
         _backgroundWorker.DoWork += DeadletterMessages_DoWork;
         _backgroundWorker.RunWorkerAsync( messageCount );
      }

      private void DeadletterMessages_DoWork( object sender, DoWorkEventArgs e )
      {
         int messageCount = (int) e.Argument;
         SubscriptionClient subscriptionClient = SubscriptionClient.CreateFromConnectionString( _connectionString, _topicName, _subscriptionName );
         for ( int i = 0; i < messageCount; i++ )
         {
            if ( _backgroundWorker.CancellationPending )
            {
               return;
            }
            try
            {
               BrokeredMessage message = subscriptionClient.Receive( TimeSpan.FromSeconds( 5 ) );
               if ( message != null )
               {
                  _logger.Info( "{0} - Deadlettering message '{1}' from active queue", i + 1, message.MessageId );
                  message.DeadLetter();
               }
               else
               {
                  // No more messages to receive so break out
                  break;
               }
            }
            catch ( MessagingException ex )
            {
               if ( !ex.IsTransient )
               {
                  _logger.Warn( "Exception while receiving and deadlettering message", ex );
                  throw;
               }
               _logger.Warn( "Transient exception while receiving and deadlettering message", ex );
            }
         }
      }

      public void MoveErrorMessages( int messageCount )
      {
         if ( IsBackgroundOperationInProgress() )
         {
            return;
         }
         _backgroundWorker = new BackgroundWorker();
         _backgroundWorker.WorkerSupportsCancellation = true;
         _backgroundWorker.DoWork += MoveErrorMessages_DoWork;
         _backgroundWorker.RunWorkerAsync( messageCount );
      }

      public void MoveErrorMessages_DoWork( object sender, DoWorkEventArgs e )
      {
         int messageCount = (int) e.Argument;
         MessagingFactory messagingFactory = MessagingFactory.CreateFromConnectionString( _connectionString );
         var deadLetterPath = SubscriptionClient.FormatDeadLetterPath( _topicName, _subscriptionName );
         MessageReceiver messageReceiver = messagingFactory.CreateMessageReceiver( deadLetterPath );

         string errorTopicName = _topicName + "_error";
         string errorSubscriptionName = _subscriptionName + "_error";
         var errorTopicService = new TopicService( _connectionString, errorTopicName );
         errorTopicService.CreateTopicIfNotExists();
         var errorSubscriptionService = new SubscriptionService( _connectionString, errorTopicName, errorSubscriptionName );
         errorSubscriptionService.CreateSubscriptionIfNotExists();

         for ( int i = 0; i < messageCount; i++ )
         {
            if ( _backgroundWorker.CancellationPending )
            {
               return;
            }
            try
            {
               BrokeredMessage message = messageReceiver.Receive( TimeSpan.FromSeconds( 5 ) );
               if ( message != null )
               {
                  _logger.Info( "{0} - Moving message '{1}' to error topic", i + 1, message.MessageId );
                  errorTopicService.SendMessage( message.Clone() );
                  message.Complete();
               }
               else
               {
                  // No more messages to receive so break out
                  break;
               }
            }
            catch ( MessagingException ex )
            {
               if ( !ex.IsTransient )
               {
                  _logger.Warn( "Exception while moving error message", ex );
                  throw;
               }
               _logger.Warn( "Transient exception while moving error message", ex );
            }
         }
      }

      public void CancelBackgroundWorker()
      {
         if ( _backgroundWorker != null )
         {
            _backgroundWorker.CancelAsync();
         }
      }

      public IEnumerable<BrokeredMessage> PeekDeadletterMessages( int messageCount )
      {
         _logger.Info( "Peeking at top '{0}' deadletter messages", messageCount );  
         MessagingFactory messagingFactory = MessagingFactory.CreateFromConnectionString( _connectionString );
         var deadLetterPath = SubscriptionClient.FormatDeadLetterPath( _topicName, _subscriptionName );
         MessageReceiver messageReceiver = messagingFactory.CreateMessageReceiver( deadLetterPath );
         var messages = new List<BrokeredMessage>();
         int messagesToProcess = messageCount;
         while ( messagesToProcess > 0 )
         {
            try
            {
               int batchSize = messagesToProcess > 250 ? 250 : messagesToProcess;
               messages.AddRange( messageReceiver.PeekBatch( batchSize ) );
               messagesToProcess = messagesToProcess - batchSize;
            }
            catch ( MessagingException ex )
            {
               if ( !ex.IsTransient )
               {
                  _logger.Warn( "Exception while peeking deadletter message", ex );
                  throw;
               }
               _logger.Warn( "Transient exception while peeking deadletter message", ex );
               // Transient exceptions should have retry logic
               throw;
            }
         }
         return messages;
      }

      public IEnumerable<BrokeredMessage> PeekActiveMessages( int messageCount )
      {
         _logger.Info( "Peeking at top '{0}' active messages", messageCount );  
         SubscriptionClient subscriptionClient = SubscriptionClient.CreateFromConnectionString( _connectionString, _topicName, _subscriptionName );
         var messages = new List<BrokeredMessage>();
         int messagesToProcess = messageCount;
         while ( messagesToProcess > 0 )
         {
            try
            {
               int batchSize = messagesToProcess > 250 ? 250 : messagesToProcess;
               messages.AddRange( subscriptionClient.PeekBatch( batchSize ) );
               messagesToProcess = messagesToProcess - batchSize;
            }
            catch ( MessagingException ex )
            {
               if ( !ex.IsTransient )
               {
                  _logger.Warn( "Exception while peeking message", ex );
                  throw;
               }
               _logger.Warn( "Transient exception while peeking message", ex );
               // Transient exceptions should have retry logic
               throw;
            }
         }
         return messages;
      }
   }
}
