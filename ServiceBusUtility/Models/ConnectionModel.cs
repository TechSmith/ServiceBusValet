using System.Collections.Generic;
using Microsoft.ServiceBus.Messaging;

namespace ServiceBusUtility.Models
{
   public class ConnectionModel
   {
      public string ConnectionString
      {
         get;
         set;
      }

      public IEnumerable<TopicDescription> Topics
      {
         get;
         set;
      }

      public IEnumerable<SubscriptionDescription> Subscriptions
      {
         get;
         set;
      }

      public string CurrentTopic
      {
         get;
         set;
      }

      public string CurrentSubscription
      {
         get;
         set;
      }
   }
}
