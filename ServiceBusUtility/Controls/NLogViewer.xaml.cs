using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows.Controls;

namespace NlogViewer
{
   public partial class NlogViewer : UserControl
   {
      public ObservableCollection<LogEventViewModel> LogEntries
      {
         get;
         private set;
      }
      public bool IsTargetConfigured
      {
         get;
         private set;
      }


      public NlogViewer()
      {
         IsTargetConfigured = false;
         LogEntries = new ObservableCollection<LogEventViewModel>();

         InitializeComponent();

         if ( !DesignerProperties.GetIsInDesignMode( this ) )
         {
            foreach ( NlogViewerTarget target in NLog.LogManager.Configuration.AllTargets.Where( t => t is NlogViewerTarget ).Cast<NlogViewerTarget>() )
            {
               IsTargetConfigured = true;
               target.LogReceived += LogReceived;
            }
         }
      }

      protected void LogReceived( NLog.Common.AsyncLogEventInfo log )
      {
         LogEventViewModel vm = new LogEventViewModel( log.LogEvent );

         Dispatcher.BeginInvoke( new Action( () =>
         {
            if ( LogEntries.Count >= 250 )
               LogEntries.RemoveAt( 0 );

            LogEntries.Add( vm );
         } ) );
      }
   }
}