using NLog;
using System;
using System.Windows.Media;

namespace NlogViewer
{
   public class LogEventViewModel
   {
      private LogEventInfo _logEventInfo;

      public LogEventViewModel( LogEventInfo logEventInfo )
      {
         _logEventInfo = logEventInfo;

         ToolTip = _logEventInfo.FormattedMessage;
         Level = _logEventInfo.Level.ToString();
         FormattedMessage = _logEventInfo.FormattedMessage;
         TimeStamp = _logEventInfo.TimeStamp;
         LoggerName = _logEventInfo.LoggerName;

         SetupColors();
      }

      private void SetupColors()
      {
         if ( _logEventInfo.Level == LogLevel.Warn )
         {
            Background = Brushes.Yellow;
            BackgroundMouseOver = Brushes.GreenYellow;
         }
         else if ( _logEventInfo.Level == LogLevel.Error )
         {
            Background = Brushes.Tomato;
            BackgroundMouseOver = Brushes.IndianRed;
         }
         else
         {
            Background = Brushes.White;
            BackgroundMouseOver = Brushes.LightGray;
         }
         Foreground = Brushes.Black;
         ForegroundMouseOver = Brushes.Black;
      }

      public string LoggerName
      {
         get;
         private set;
      }
      public string Level
      {
         get;
         private set;
      }
      public string FormattedMessage
      {
         get;
         private set;
      }
      public DateTime TimeStamp
      {
         get;
         private set;
      }
      public string ToolTip
      {
         get;
         private set;
      }
      public SolidColorBrush Background
      {
         get;
         private set;
      }
      public SolidColorBrush Foreground
      {
         get;
         private set;
      }
      public SolidColorBrush BackgroundMouseOver
      {
         get;
         private set;
      }
      public SolidColorBrush ForegroundMouseOver
      {
         get;
         private set;
      }
   }
}
