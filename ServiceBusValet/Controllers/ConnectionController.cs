using System;
using System.Collections.Generic;
using System.Windows;
using Microsoft.ServiceBus.Messaging;
using TechSmith.ServiceBusValet.Models;
using TechSmith.ServiceBusValet.Services;
using TechSmith.ServiceBusValet.ViewModels;

namespace TechSmith.ServiceBusValet.Controllers
{
   public class ConnectionController
   {
      private readonly ConnectionWindow _connectionWindow;
      private readonly ConnectionViewModel _connectionViewModel;
      private ServiceBusEnvironments _environments;

      public ConnectionController( ConnectionWindow connectionWindow )
      {
         _connectionWindow = connectionWindow;
         _connectionViewModel = new ConnectionViewModel();
      }

      public void UpdateEnvironmentsList( string environmentsFilePath )
      {
         _environments = new ServiceBusEnvironments( environmentsFilePath );

         _connectionViewModel.EnvironmentNames.Clear();

         foreach ( var environment in _environments.GetNames() )
         {
            _connectionViewModel.EnvironmentNames.Add( environment );
         }
         _connectionWindow.ComboEnvironment.ItemsSource = _connectionViewModel.EnvironmentNames;
         _connectionWindow.ComboEnvironment.SelectedIndex = 0;
      }

      public void SelectEnvironment( string environment )
      {
         string connectionString = string.Empty;

         if ( !string.IsNullOrWhiteSpace( environment ) )
         {
            connectionString = _environments.GetConnectionString( environment );
         }

         _connectionViewModel.ConnectionString = connectionString;
         _connectionWindow.TextConnectionString.Text = _connectionViewModel.ConnectionString;
      }

      public bool EstablishConnection()
      {
         var connectionService = new ConnectionService( _connectionViewModel.ConnectionString );

         IEnumerable<TopicDescription> topicDescriptions;
         try
         {
            topicDescriptions = connectionService.GetTopics();
         }
         catch ( Exception ex )
         {
            string messageBoxCaption = string.Format( "Connection Error" );
            string messageBoxText = string.Format( "There was an error connecting to the specified service bus. Message: {0}", ex.Message );
            var messageBoxButton = MessageBoxButton.OK;
            var messageBoxIcon = MessageBoxImage.Error;
            MessageBox.Show( messageBoxText, messageBoxCaption, messageBoxButton, messageBoxIcon );
            return false;
         }

         Cache.Connection = new ConnectionModel
         {
            ConnectionString = _connectionViewModel.ConnectionString,
            Topics = topicDescriptions
         };
         return true;
      }

      public void SwitchToMainWindow()
      {
         var mainWindow = new MainWindow();
         App.Current.MainWindow = mainWindow;
         _connectionWindow.Close();
         mainWindow.Show();
      }
   }
}
