using System.Collections.Generic;
using Microsoft.ServiceBus.Messaging;

namespace ServiceBusUtility.Models
{
   public static class Cache
   {
      public static ConnectionModel Connection
      {
         get;
         set;
      }

      public static IEnumerable<BrokeredMessage> DeadletterMessages
      {
         get;
         set;
      }

      public static Dictionary<string, string> DeadletterMessageBodies
      {
         get;
         set;
      }

      public static IEnumerable<BrokeredMessage> ActiveMessages
      {
         get;
         set;
      }

      public static Dictionary<string, string> ActiveMessageBodies
      {
         get;
         set;
      }
   }
}
