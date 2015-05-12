# DesktopApplicationInsights
Enables easy and automatic usage of Application Insights with your Windows Forms or other Windows Desktop application.
## Setup
1. Create a new Windows desktop application (Forms, Console, etc)
2. Add the `BC3Technologies.DesktopApplicationInsights` [nuget package](https://www.nuget.org/packages/BC3Technologies.DesktopApplicationInsights/) to the project
3. Create a `TelemetryClient` object in your code with:
```
var myClient = Telemetry.CreateClient("<name to give the client>", "<instrumentation key from App Insights project>");
```
In most Desktop apps, if you plan to use just one client throughout the app you can put this line in `Program.cs`' `Main()` method.
This does the following: 

1. Pulls version number, OS, screen resolutions, etc in to the context of your Telemetry calls.
2. Sets App Insights to field Unhandled exceptions from your application and log them as Critical Exceptions in your App Insights instance.
3. Flushes all telemetry events to App Insights upon Application exit.

If you need to later get a client you've created, simply
```
var client = Telemetry.GetClient("<name you gave the client>")
```
An always-correct method of `GetOrCreateClient(name, key)` is also available.
## Usage
To log a Telemetry event, it's as easy as
```csharp
myClient.TrackEvent(string);
```
This goes directly against the Application Insights Core API, so a `properties`, `metrics`, etc are all also present, etc.
## Automatic Page View tracking
You can automatically track "Page Views" in a Windows Forms app by making your Form inherit from `TelemetryForm` and setting its `TelemetryClientName` property. By doing this you'll get Page View events logged to App Insights **with duration information**.
### Example
```csharp
public partial class Form1 : TelemetryForm
{
    public Form1()
    {
        this.TelemetryClientName = "form1Client";
```
Or you can even use the Designer for your Form to set the property even more easily.
## Logging Handled Exceptions
To make your life easier and reduce boilerplate code, logging a **handled** exception can be done simply with
```csharp
catch (Exception ex)
{
    // you've done var myClient = Telemetry.CreateClient() somewhere
    myClient.LogHandledException(ex);
}
```
Optional parameters exist to specify the severity, a custom message, and any data points you want attached to the logged exception in App Insights. By default exceptions logged via `LogHandledException` have a SeverityLevel of `Error`.
## Timing custom events
To simply the timing of a custom event, use the `TimedTelemetryEvent` Disposable class:
```csharp
// you've done var myClient = Telemetry.CreateClient() somewhere
using (new TimedTelemetryEvent(myClient, "EventFire"))
{
    throw new ApplicationException("Boomshackalacka!");
}
```
This creates a Custom Event in App Insights with the name "EventFire" and logs the duration it took to, in his case, throw the exception (which gets logged as an Unhandled Exception as it crashes the app).
## Tracking button clicks
You can very easily log - and optionall *time* - button click events by having your `Button` inherit from `TelemetryButton` and set the `EventName` and `TelemetryClientName` properties on your Button.
Your button will now automatically execute `TrackEvent` when it's clicked. If you set the `IsTimed` property to `true` it will also log duration metrics. The Duration property that's logged to Application Insights will be called `[EventName]_Duration` so you can filter these button click durations very easily in your dashboard.
