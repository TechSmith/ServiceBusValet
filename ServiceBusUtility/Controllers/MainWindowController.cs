using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Windows;
using System.Xml;
using Microsoft.ServiceBus.Messaging;
using NLog;
using ServiceBusUtility.Models;
using ServiceBusUtility.Services;
using ServiceBusUtility.ViewModels;

namespace ServiceBusUtility.Controllers
{
   public class MainWindowController
   {
      private static readonly Logger _logger = LogManager.GetLogger( "Main" );  

      private readonly MainWindow _mainWindow;
      private readonly MainWindowViewModel _mainWindowViewModel;

      private SubscriptionService _subscriptionService;

      public MainWindowController( MainWindow mainWindow )
      {
         _mainWindow = mainWindow;
         _mainWindowViewModel = new MainWindowViewModel();
      }

      public void InitializeView()
      {
         foreach ( var topic in Cache.Connection.Topics )
         {
            _mainWindowViewModel.TopicNames.Add( topic.Path );
         }

         string serviceBusName = Cache.Connection.ConnectionString.Split(';').First();
         _mainWindow.Title = serviceBusName;
         _logger.Debug( "Connected to: {0}", serviceBusName );

         _mainWindow.ComboTopic.ItemsSource = _mainWindowViewModel.TopicNames;
         _mainWindow.ComboTopic.SelectedIndex = 0;
      }

      public void SelectTopic( string topicName )
      {
         if ( topicName.Contains( "..." ) )
         {
            return;
         }

         _logger.Debug( "Switching to topic '{0}'", topicName );

         Cache.Connection.CurrentTopic = topicName;
         
         ClearSubscriptions();
         ClearDeadletterMessages();
         ClearMessageType();
         
         var topicService = new TopicService( Cache.Connection.ConnectionString, topicName );
         Cache.Connection.Subscriptions = topicService.GetSubscriptions();
         foreach ( var subscription in Cache.Connection.Subscriptions )
         {
            _mainWindowViewModel.SubscriptionNames.Add( subscription.Name );
         }
         _mainWindow.ComboSubscription.ItemsSource = _mainWindowViewModel.SubscriptionNames;
         _mainWindow.ComboSubscription.SelectedIndex = 0;
      }

      private void ClearSubscriptions()
      {
         Cache.Connection.Subscriptions = null;
         Cache.Connection.CurrentSubscription = null;
         _mainWindowViewModel.SubscriptionNames.Clear();
         _mainWindowViewModel.SubscriptionNames.Add( "Select a subscription..." );
      }

      private void ClearDeadletterMessages()
      {
         Cache.DeadletterMessages = null;
         Cache.DeadletterMessageBodies = new Dictionary<string, string>();
         _mainWindowViewModel.MessageProperties.Clear();
         _mainWindowViewModel.MessageIds.Clear();
         _mainWindow.TextMessageBody.Text = string.Empty;
      }

      private void ClearActiveMessages()
      {
         Cache.ActiveMessages = null;
         Cache.ActiveMessageBodies = new Dictionary<string, string>();
         _mainWindowViewModel.MessageProperties.Clear();
         _mainWindowViewModel.MessageIds.Clear();
         _mainWindow.TextMessageBody.Text = string.Empty;   
      }

      private void ClearMessageType()
      {
         if ( _mainWindow.RadioActiveMessages.IsChecked.HasValue && _mainWindow.RadioActiveMessages.IsChecked.Value )
         {
            _mainWindow.RadioActiveMessages.IsChecked = false;
         }
         if ( _mainWindow.RadioDeadletterMessages.IsChecked.HasValue && _mainWindow.RadioDeadletterMessages.IsChecked.Value )
         {
            _mainWindow.RadioDeadletterMessages.IsChecked = false;
         }
      }

      public void SelectSubscription( string subscriptionName )
      {
         if ( subscriptionName.Contains( "..." ) )
         {
            return;
         }

         _logger.Debug( "Switching to subscription '{0}'", subscriptionName );

         ClearDeadletterMessages();
         ClearMessageType();

         Cache.Connection.CurrentSubscription = subscriptionName;
         string currentConnection = Cache.Connection.ConnectionString;
         string currentTopic = Cache.Connection.CurrentTopic;
         string currentSubscription = Cache.Connection.CurrentSubscription;
         if ( string.IsNullOrWhiteSpace( currentConnection ) || string.IsNullOrWhiteSpace( currentTopic ) || string.IsNullOrWhiteSpace( currentSubscription ) )
         {
            _subscriptionService = null;
         }
         else
         {
            _subscriptionService = new SubscriptionService( currentConnection, currentTopic, currentSubscription );
         }
      }

      public void ChangeMessageMode( MessageKind messageKind )
      {
         switch ( messageKind )
         {
            case MessageKind.Active:
            {
               _mainWindowViewModel.MessageMode = MessageKind.Active;
               _mainWindowViewModel.NumberOfMessagesDisplayed = 10;
               _mainWindow.TextNumberOfMessagesDisplayed.Text = _mainWindowViewModel.NumberOfMessagesDisplayed.ToString();
               RefreshMessages();
               break;
            }
            case MessageKind.Deadletter:
            {
               _mainWindowViewModel.MessageMode = MessageKind.Deadletter; 
               _mainWindowViewModel.NumberOfMessagesDisplayed = 10;
               _mainWindow.TextNumberOfMessagesDisplayed.Text = _mainWindowViewModel.NumberOfMessagesDisplayed.ToString();
               RefreshMessages();
               break;
            }
            default:
            {
               _mainWindowViewModel.MessageMode = MessageKind.None; 
               return;
            }
         }
      }

      public void RefreshMessages()
      {
         if ( _subscriptionService == null )
         {
            return;
         }

         _mainWindowViewModel.NumberOfMessagesDisplayed = Convert.ToInt32( _mainWindow.TextNumberOfMessagesDisplayed.Text );

         _mainWindow.ButtonMoveErrorMessages.Content = string.Format( "Move Top {0} Messages To Error Topic", _mainWindowViewModel.NumberOfMessagesDisplayed );

         switch ( _mainWindowViewModel.MessageMode )
         {
            case MessageKind.Deadletter:
            {
               RefreshTotalMessageCount( _subscriptionService, _mainWindowViewModel.MessageMode );

               ClearDeadletterMessages();

               _mainWindow.ButtonResendMessages.Content = string.Format( "Resend Top {0} Deadletter Messages", _mainWindowViewModel.NumberOfMessagesDisplayed );

               Cache.DeadletterMessages = _subscriptionService.PeekDeadletterMessages( _mainWindowViewModel.NumberOfMessagesDisplayed );
               foreach ( var message in Cache.DeadletterMessages )
               {
                  _mainWindowViewModel.MessageIds.Add( message.MessageId );
               }
               _mainWindow.ListMessages.ItemsSource = _mainWindowViewModel.MessageIds;
               break;
            }
            case MessageKind.Active:
            {
               RefreshTotalMessageCount( _subscriptionService, _mainWindowViewModel.MessageMode );

               ClearActiveMessages();

               _mainWindow.ButtonResendMessages.Content = string.Format( "Deadletter Top {0} Active Messages", _mainWindowViewModel.NumberOfMessagesDisplayed );

               Cache.ActiveMessages = _subscriptionService.PeekActiveMessages( _mainWindowViewModel.NumberOfMessagesDisplayed );
               foreach ( var message in Cache.ActiveMessages )
               {
                  _mainWindowViewModel.MessageIds.Add( message.MessageId );
               }
               _mainWindow.ListMessages.ItemsSource = _mainWindowViewModel.MessageIds;
               break;
            }
         }

         _logger.Debug( "List updated to show '{0}' messages", _mainWindow.ListMessages.Items.Count ); 
      }
      
      private void RefreshTotalMessageCount( SubscriptionService subscriptionService, MessageKind messageKind )
      {
         var messageCountDetails = subscriptionService.GetMessageCountDetails();
         switch ( messageKind )
         {
            case MessageKind.Deadletter:
            {
               _mainWindowViewModel.TotalNumberOfMessages = messageCountDetails.DeadLetterMessageCount;
               break;
            }
            case MessageKind.Active:
            {
               _mainWindowViewModel.TotalNumberOfMessages = messageCountDetails.ActiveMessageCount;
               break;
            }
         }
         _mainWindow.TextTotalMessage.Text = _mainWindowViewModel.TotalNumberOfMessages.ToString();
      }

      public void ShowAllMessages()
      {
         int maxMessagesToShow = 10000;
         _logger.Debug( "Showing up to {0} top messages", maxMessagesToShow );  
         _mainWindowViewModel.NumberOfMessagesDisplayed = _mainWindowViewModel.TotalNumberOfMessages > maxMessagesToShow ? maxMessagesToShow : (int) _mainWindowViewModel.TotalNumberOfMessages;
         _mainWindow.TextNumberOfMessagesDisplayed.Text = _mainWindowViewModel.NumberOfMessagesDisplayed.ToString();
         RefreshMessages();
      }

      public void SelectMessage( string messageId )
      {
         Dictionary<string, string> messageBodies;
         IEnumerable<BrokeredMessage> brokeredMessages;

         switch ( _mainWindowViewModel.MessageMode )
         {
            case MessageKind.Deadletter:
            {
               messageBodies = Cache.DeadletterMessageBodies;
               brokeredMessages = Cache.DeadletterMessages;
               break;
            }
            case MessageKind.Active:
            {
               messageBodies = Cache.ActiveMessageBodies;
               brokeredMessages = Cache.ActiveMessages;
               break;
            }
            default:
            {
               return;
            }
         }

         string cachedMessageBody;
         messageBodies.TryGetValue( messageId, out cachedMessageBody );
         if ( cachedMessageBody != null )
         {
            _mainWindow.TextMessageBody.Text = cachedMessageBody;
            RefreshMessageProperties( brokeredMessages, messageId );
            return;
         }

         // We should use SingleOrDefault except it seems like MessageId does not have to be unique
         BrokeredMessage brokeredMessage = brokeredMessages.FirstOrDefault( m => m.MessageId == messageId );
         if ( brokeredMessage == null )
         {
            return;
         }

         RefreshMessageProperties( brokeredMessage );

         string messageBody;
         var stream = brokeredMessage.GetBody<Stream>();
         var serializer = new DataContractSerializer( typeof(string) );
         XmlDictionaryReader reader = XmlDictionaryReader.CreateBinaryReader( stream, XmlDictionaryReaderQuotas.Max );
         try
         {
            messageBody = serializer.ReadObject( reader ) as string;
         }
         catch ( SerializationException )
         {
            _logger.Warn( "Exception while reading message body. Falling back to raw stream." );
            var streamReader = new StreamReader( stream );
            messageBody = streamReader.ReadToEnd();
         }
         messageBodies.Add( messageId, messageBody );
         _mainWindow.TextMessageBody.Text = messageBody;
      }

      private void RefreshMessageProperties( IEnumerable<BrokeredMessage> brokeredMessages, string messageId )
      {
         BrokeredMessage brokeredMessage = brokeredMessages.FirstOrDefault( m => m.MessageId == messageId );
         RefreshMessageProperties( brokeredMessage );
      }

      private void RefreshMessageProperties( BrokeredMessage brokeredMessage )
      {
         if ( brokeredMessage == null )
         {
            return;
         }

         _mainWindowViewModel.MessageProperties = MessageProperty.CreateCollectionFromBrokeredMessage( brokeredMessage );
         _mainWindow.DataMessageProperties.ItemsSource = _mainWindowViewModel.MessageProperties;
      }

      public void ResendMessages()
      {
         if ( _subscriptionService == null )
         {
            return;
         }

         string messageBoxCaption = string.Format( "Process {0} Messages?", _mainWindowViewModel.NumberOfMessagesDisplayed );
         string messageBoxText = string.Format( "Are you sure you want to process these {0} messages?", _mainWindowViewModel.NumberOfMessagesDisplayed );
         var messageBoxButton = MessageBoxButton.YesNo;
         var messageBoxIcon = MessageBoxImage.Warning;
         MessageBoxResult result = MessageBox.Show( messageBoxText, messageBoxCaption, messageBoxButton, messageBoxIcon );
         if ( result == MessageBoxResult.No )
         {
            return;
         }
         
         switch ( _mainWindowViewModel.MessageMode )
         {
            case MessageKind.Deadletter:
            {
               _subscriptionService.ResendDeadletterMessages( _mainWindowViewModel.NumberOfMessagesDisplayed );
               // Only refresh on completed
               RefreshMessages();
               break;
            }
            case MessageKind.Active:
            {
               _subscriptionService.DeadletterMessages( _mainWindowViewModel.NumberOfMessagesDisplayed );
               // Only refresh on completed
               RefreshMessages();
               break;
            }
         }
      }

      public void MoveErrorMessages()
      {
         if ( _mainWindowViewModel.MessageMode != MessageKind.Deadletter )
         {
            string dlMessageBoxCaption = "Only Deadletter Messages Can Be Moved";
            string dlMessageBoxText = "Only deadletter messages can be moved to the error topic";
            var dlMessageBoxButton = MessageBoxButton.OK;
            var dlMessageBoxIcon = MessageBoxImage.Warning;
            MessageBox.Show( dlMessageBoxText, dlMessageBoxCaption, dlMessageBoxButton, dlMessageBoxIcon );
            return;
         }
         string messageBoxCaption = string.Format( "Move {0} Messages To Error Topic?", _mainWindowViewModel.NumberOfMessagesDisplayed );
         string messageBoxText = string.Format( "Are you sure you want to move these {0} messages to the error topic?", _mainWindowViewModel.NumberOfMessagesDisplayed );
         var messageBoxButton = MessageBoxButton.YesNo;
         var messageBoxIcon = MessageBoxImage.Warning;
         MessageBoxResult result = MessageBox.Show( messageBoxText, messageBoxCaption, messageBoxButton, messageBoxIcon );
         if ( result == MessageBoxResult.No )
         {
            return;
         }

         _subscriptionService.MoveErrorMessages( _mainWindowViewModel.NumberOfMessagesDisplayed );
      }

      public void ChangeConnection()
      {
         var connectionWindow = new ConnectionWindow();
         App.Current.MainWindow = connectionWindow;
         _mainWindow.Close();
         connectionWindow.Show();
      }

      public void StopCurrentJob()
      {
         if ( _subscriptionService == null )
         {
            return;
         }
         _logger.Info( "Stopping currently running job" );
         _subscriptionService.CancelBackgroundWorker();
      }
   }
}
