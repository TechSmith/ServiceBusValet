using System.Collections.ObjectModel;
using Microsoft.ServiceBus.Messaging;

namespace ServiceBusUtility.Models
{
   public class MessageProperty
   {
      public static ObservableCollection<MessageProperty> CreateCollectionFromBrokeredMessage( BrokeredMessage brokeredMessage )
      {
         var collection = new ObservableCollection<MessageProperty>();
         AddPropertyToCollection( collection, "MessageId", brokeredMessage.MessageId );
         AddPropertyToCollection( collection, "CorrelationId", brokeredMessage.CorrelationId );
         AddPropertyToCollection( collection, "To", brokeredMessage.To );
         AddPropertyToCollection( collection, "ReplyTo", brokeredMessage.ReplyTo );
         AddPropertyToCollection( collection, "SessionId", brokeredMessage.SessionId );
         AddPropertyToCollection( collection, "Label", brokeredMessage.Label );
         AddPropertyToCollection( collection, "ContentType", brokeredMessage.ContentType );
         AddPropertyToCollection( collection, "ReplyToSessionId", brokeredMessage.ReplyToSessionId );
         AddPropertyToCollection( collection, "TimeToLive", brokeredMessage.TimeToLive.ToString("h'h 'm'm 's's'") );
         AddPropertyToCollection( collection, "ScheduledEnqueueTimeUtc", brokeredMessage.ScheduledEnqueueTimeUtc.ToString() );
         AddPropertyToCollection( collection, "PartitionKey", brokeredMessage.PartitionKey );
         AddPropertyToCollection( collection, "EnqueuedTimeUtc", brokeredMessage.EnqueuedTimeUtc.ToString() );
         AddPropertyToCollection( collection, "SequenceNumber", brokeredMessage.SequenceNumber.ToString() );
         AddPropertyToCollection( collection, "DeliveryCount", brokeredMessage.DeliveryCount.ToString() );
         AddPropertyToCollection( collection, "EnqueuedSequenceNumber", brokeredMessage.EnqueuedSequenceNumber.ToString() );
         AddPropertyToCollection( collection, "ViaPartitionKey", brokeredMessage.ViaPartitionKey );
         AddPropertyToCollection( collection, "ForcePersistence", brokeredMessage.ForcePersistence.ToString() );
         return collection;
      }

      private static void AddPropertyToCollection( ObservableCollection<MessageProperty> collection, string propertyName, string propertyValue )
      {
         if ( !string.IsNullOrWhiteSpace( propertyValue ) )
         {
            collection.Add( new MessageProperty
            {
               Name = propertyName,
               Value = propertyValue
            } );
         }
      }

      public string Name
      {
         get;
         set;
      }

      public string Value
      {
         get;
         set;
      }
   }
}

