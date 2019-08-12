using System.Collections.Specialized;
using System.Windows;
using System.Windows.Controls;

namespace NLogViewer
{
   public class SelectorExtenders : DependencyObject
   {

      public static bool GetIsAutoscroll( DependencyObject sender )
      {
         return (bool) sender.GetValue( IsAutoscrollProperty );
      }

      public static void SetIsAutoscroll( DependencyObject sender, bool value )
      {
         sender.SetValue( IsAutoscrollProperty, value );
      }

      public static readonly DependencyProperty IsAutoscrollProperty = DependencyProperty.RegisterAttached( "IsAutoscroll", typeof( bool ),
         typeof( SelectorExtenders ), new UIPropertyMetadata( default( bool ), OnIsAutoscrollChanged ) );

      public static void OnIsAutoscrollChanged( DependencyObject sender, DependencyPropertyChangedEventArgs e )
      {
         var newValue = (bool) e.NewValue;
         var listBox = sender as ListBox;
         if ( listBox == null )
         {
            return;
         }
         var itemCollection = listBox.Items;
         var data = itemCollection.SourceCollection as INotifyCollectionChanged;

         var autoscroller = new NotifyCollectionChangedEventHandler( ( notifySender, eventArgs ) =>
         {
            object selectedItem = default( object );
            switch ( eventArgs.Action )
            {
               case NotifyCollectionChangedAction.Add:
               case NotifyCollectionChangedAction.Move:
                  selectedItem = eventArgs.NewItems[eventArgs.NewItems.Count - 1];
                  break;
               case NotifyCollectionChangedAction.Remove:
                  if ( itemCollection.Count < eventArgs.OldStartingIndex )
                  {
                     selectedItem = itemCollection[eventArgs.OldStartingIndex - 1];
                  }
                  else if ( itemCollection.Count > 0 )
                     selectedItem = itemCollection[0];
                  break;
               case NotifyCollectionChangedAction.Reset:
                  if ( itemCollection.Count > 0 )
                     selectedItem = itemCollection[0];
                  break;
            }

            if ( selectedItem != default( object ) )
            {
               itemCollection.MoveCurrentTo( selectedItem );
               listBox.ScrollIntoView( selectedItem );
            }
         } );

         if ( newValue )
            data.CollectionChanged += autoscroller;
         else
            data.CollectionChanged -= autoscroller;

      }
   }
}
