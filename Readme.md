# Service Bus Valet #

A WPF application used to inspect and reprocess Deadletter messages in Service Bus Topics / Subscriptions.

## Known Issues ##

- When dealing with Deadletter queues you are not able to receive a specific message even if you know unique properties of that message. Receives always grab the oldest message or the first message that had arrived FIFO style. This makes the UI a bit awkward as you can peek at messages but not select a specific message from the list of peeked messages to resend. Instead you have to resend the top X messages.
- Re-sending messages is done through a background worker which means that the UI remains responsive. You can also stop the batch using the Stop menu if you decide to abort processing early. However the UI does not prevent you from starting a second batch of message processing while the first is in progress however this will not currently work and should not be attempted.
- Using the Microsoft Service Bus SDK you cannot call GetBody on a BrokeredMessage more than once so the results must be cached the first time. This is the purpose of the message body dictionaries in models/cache folder. The messages bodies are stored in the dictionary by MessageId however I found out later that MessageIds are not required to be Unique so this may not always work. For our services the MessageId is a GUID so this will be acceptable.

## NLog ##

NLog is used with a custom target that will output messages in realtime to a WPF control. This provides status of batch processing jobs as they are in flight as well as any errors or exceptions. By adding additional targets to the NLog.config file it would be trivial to also output messages to a log file, sql database or any other target supported by NLog of which there are many: [https://github.com/nlog/nlog/wiki/Targets](https://github.com/nlog/nlog/wiki/Targets "NLog Targets")

## Code Organization ##

### Controllers ###

The purpose of the Controllers folder is to provide a controller for each XAML view. All of the logic required by the page will be present here and the code-behind for the XAML will call methods on its respective controller

### Controls ###

The Controls folder contains the modified source code for a custom WPF control called the NLogViewer. The original source can be found on Github: [https://github.com/erizet/NlogViewer](https://github.com/erizet/NlogViewer "NLogViewer")

The NLogViewer control is responsible for displaying log messages in a realtime fashion. We use this to show progress and errors of any batch jobs that are processing.

The control was custom modified so that it will auto scroll as new messages come in, thus keeping new messages always in view.

### Models ###

The purpose of the Models folder is to contain classes that store the real state content of the application.

### Services ###

Services are responsible for making the calls to Azure ServiceBus to deal with connections, topics and subscriptions.

### ViewModel ###

The purpose of the ViewModel folder is to provide the actual data for what will be displayed in a XAML view.
