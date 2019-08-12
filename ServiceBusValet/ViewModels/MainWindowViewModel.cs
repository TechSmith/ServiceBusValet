using System.Collections.ObjectModel;
using ServiceBusUtility.Models;

namespace ServiceBusUtility.ViewModels
{
   public class MainWindowViewModel
   {
      public MainWindowViewModel()
      {
         TopicNames = new ObservableCollection<string>();
         TopicNames.Add( "Select a topic..." );
         SubscriptionNames = new ObservableCollection<string>();
         MessageIds = new ObservableCollection<string>();
         MessageProperties = new ObservableCollection<MessageProperty>();
      }

      public MessageKind MessageMode
      {
         get;
         set;
      }

      public ObservableCollection<string> TopicNames
      {
         get;
         set;
      }

      public ObservableCollection<string> SubscriptionNames
      {
         get;
         set;
      }

      public ObservableCollection<string> MessageIds
      {
         get;
         set;
      }

      public ObservableCollection<MessageProperty> MessageProperties
      {
         get;
         set;
      }

      public int NumberOfMessagesDisplayed
      {
         get;
         set;
      }

      public long TotalNumberOfMessages
      {
         get;
         set;
      }
   }
}
