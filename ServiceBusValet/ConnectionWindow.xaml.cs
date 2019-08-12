using System.Windows;
using Microsoft.Win32;
using TechSmith.ServiceBusValet.Controllers;

namespace TechSmith.ServiceBusValet
{
   public partial class ConnectionWindow : Window
   {
      private ConnectionController _connectionController;

      public ConnectionWindow()
      {
         InitializeComponent();
      }

      private void WindowConnection_Loaded( object sender, RoutedEventArgs e )
      {
         _connectionController = new ConnectionController( this );
      }

      private void ComboEnvironment_SelectionChanged( object sender, System.Windows.Controls.SelectionChangedEventArgs e )
      {
         _connectionController.SelectEnvironment( ComboEnvironment.SelectedItem != null ? ComboEnvironment.SelectedItem.ToString() : string.Empty );
      }

      private void ButtonConnect_Click( object sender, RoutedEventArgs e )
      {
         if ( _connectionController.EstablishConnection() )
         {
            _connectionController.SwitchToMainWindow();
         }
      }

      private void ButtonBrowse_Click( object sender, RoutedEventArgs e )
      {
         OpenFileDialog dlg = new OpenFileDialog();
         dlg.DefaultExt = ".csv";
         dlg.Filter = "CSV (Comma delimited) (*.csv)|*.csv";

         bool? result = dlg.ShowDialog();

         if ( result == true )
         {
            EnvironmentsFilePath.Text = dlg.FileName;
            _connectionController.UpdateEnvironmentsList( dlg.FileName );
         }
      }
   }
}
