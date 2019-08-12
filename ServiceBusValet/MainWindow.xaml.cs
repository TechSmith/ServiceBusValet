using System.Windows;
using System.Windows.Controls;
using TechSmith.ServiceBusValet.Controllers;
using TechSmith.ServiceBusValet.Models;

namespace TechSmith.ServiceBusValet
{
   public partial class MainWindow : Window
   {
      private MainWindowController _mainWindowController;

      public MainWindow()
      {
         _mainWindowController = new MainWindowController( this );
         InitializeComponent();
      }

      private void WindowMain_Loaded( object sender, RoutedEventArgs e )
      {
         _mainWindowController.InitializeView();
      }

      private void ComboTopic_SelectionChanged( object sender, SelectionChangedEventArgs e )
      {
         if ( ComboTopic.SelectedItem != null )
         {
            _mainWindowController.SelectTopic( ComboTopic.SelectedItem.ToString() );
         }
      }

      private void ComboSubscription_SelectionChanged( object sender, SelectionChangedEventArgs e )
      {
         if ( ComboSubscription.SelectedItem != null )
         {
            _mainWindowController.SelectSubscription( ComboSubscription.SelectedItem.ToString() );
         }
      }

      private void RadioMessageType_Checked( object sender, RoutedEventArgs e )
      {
         var radioButton = sender as RadioButton;
         var messageKind = MessageKind.None;
         if ( radioButton != null )
         {
            string radioButtonSelection = radioButton.Content.ToString().ToLower();
            if ( radioButtonSelection.Contains( "active" ) )
            {
               messageKind = MessageKind.Active;
            }
            else if ( radioButtonSelection.Contains( "deadletter" ) )
            {
               messageKind = MessageKind.Deadletter;
            }
         }
         _mainWindowController.ChangeMessageMode( messageKind );
      }

      private void ListMessages_SelectionChanged( object sender, SelectionChangedEventArgs e )
      {
         if ( ListMessages.SelectedItem != null )
         {
            _mainWindowController.SelectMessage( ListMessages.SelectedItem.ToString() );
         }
      }

      private void ButtonRefresh_Click( object sender, RoutedEventArgs e )
      {
         _mainWindowController.RefreshMessages();
      }

      private void ButtonShowAll_Click( object sender, RoutedEventArgs e )
      {
         _mainWindowController.ShowAllMessages();
      }

      private void ButtonResendMessages_Click( object sender, RoutedEventArgs e )
      {
         _mainWindowController.ResendMessages();
      }
      private void ButtonMoveErrorMessages_Click( object sender, RoutedEventArgs e )
      {
         _mainWindowController.MoveErrorMessages();
      }

      private void MenuChangeConnection_Click( object sender, RoutedEventArgs e )
      {
         _mainWindowController.ChangeConnection();
      }

      private void MenuStopJob_Click( object sender, RoutedEventArgs e )
      {
         _mainWindowController.StopCurrentJob();
      }
   }
}
