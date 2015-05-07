# DesktopApplicationInsights
Enables easy and automatic usage of Application Insights with your Windows Forms or other Windows Desktop application.
## Setup
1. Create a new Windows desktop application
2. Augment `Main()` as follows:
```csharp
static void Main()
{
    Telemetry.ConfigureApplication(<your instrumentation key here>);

    Application.EnableVisualStyles();
    Application.SetCompatibleTextRenderingDefault(false);
    Application.Run(new Form1());
}
```

This does the following: 

1. Pulls version number, OS, screen resolutions, etc in to the context of your Telemetry calls.
2. Sets App Insights to field Unhandled exceptions from your application and log them as Critical Exceptions in your App Insights instance.
3. Flushes all telemetry events to App Insights upon Application exit.

## Usage
To log a Telemetry event, it's as easy as
```csharp
Telemetry.Client.TrackEvent(string);
```
This goes directly against the Application Insights Core API, so a `properties` parameter is also present, etc.

## Automatic Page View tracking
You can automatically track "Page Views" in a Windows Forms app by making your Form inherit from `TelemetryForm`. By doing this you'll get Page View events logged to App Insights **with duration information**.
### Example
```csharp
public partial class Form1 : TelemetryForm
{
...
```

## Logging Handled Exceptions
To make your life easier and reduce boilerplate code, logging a **handled** exception can be done simply with
```csharp
catch (Exception ex)
{
    Telemetry.LogHandledException(ex);
}
```
Optional parameters exist to specify the severity, a custom message, and any data points you want attached to the logged exception in App Insights. By default exceptions logged via `LogHandledException` have a SeverityLevel of `Error`.

## Timing custom events
To simply the timing of a custom event, use the `TimedTelemetryEvent` Disposable class:
```csharp
using (new TimedTelemetryEvent("EventFire"))
{
    throw new ApplicationException("Boomshackalacka!");
}
```
This creates a Custom Event in App Insights with the name "EventFire" and logs the duration it took to, in his case, throw the exception (which gets logged as an Unhandled Exception as it crashes the app).
