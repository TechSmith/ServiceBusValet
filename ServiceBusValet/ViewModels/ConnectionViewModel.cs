﻿using System.Collections.ObjectModel;

namespace TechSmith.ServiceBusValet.ViewModels
{
   public class ConnectionViewModel
   {
      public ConnectionViewModel()
      {
         EnvironmentNames = new ObservableCollection<string>();
         ConnectionString = string.Empty;
      }

      public ObservableCollection<string> EnvironmentNames
      {
         get;
         set;
      }

      public string ConnectionString
      {
         get;
         set;
      }
   }
}
