using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.ApplicationInsights.Extensibility;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;
using System.Windows.Forms;

namespace DesktopApplicationInsights
{
    public static class Telemetry
    {
        private static TelemetryInitializer _initializer = new TelemetryInitializer(Assembly.GetCallingAssembly());
        public static TelemetryClient Client { get; private set; }

        static Telemetry()
        {
            Client = new TelemetryClient();
        }

        public static void ConfigureApplication(string instrumentationKey)
        {
            _initializer = new TelemetryInitializer(Assembly.GetCallingAssembly());

            TelemetryConfiguration.Active.InstrumentationKey = instrumentationKey;
            TelemetryConfiguration.Active.ContextInitializers.Add(_initializer);

            Application.ThreadException += (s, e) =>
            {
                Telemetry.Client.TrackException(new ExceptionTelemetry(e.Exception)
                {
                    HandledAt = ExceptionHandledAt.Unhandled,
                    SeverityLevel = SeverityLevel.Critical,
                });

                throw e.Exception;
            };

            Application.ApplicationExit += (s, e) =>
            {
                Telemetry.Client.Flush();
            };
        }

        public static void LogHandledException(Exception ex, SeverityLevel severityLevel = SeverityLevel.Error,
            string message = null, IDictionary<string, string> data = null)
        {
            var o = new ExceptionTelemetry(ex)
            {
                HandledAt = ExceptionHandledAt.UserCode,
                SeverityLevel = severityLevel
            };

            o.Properties.Add("LogMessage", message);
            foreach (var p in data ?? new Dictionary<string, string>())
            {
                o.Properties.Add(p);
            }

            Telemetry.Client.TrackException(o);
        }
    }

    class TelemetryInitializer : IContextInitializer
    {
        private readonly Assembly _sourceAssembly;

        public TelemetryInitializer(Assembly sourceAssembly)
        {
            _sourceAssembly = sourceAssembly;
        }

        public void Initialize(TelemetryContext context)
        {
            context.Component.Version = _sourceAssembly.GetName().Version.ToString();

            context.Device.Language = CultureInfo.CurrentUICulture.IetfLanguageTag;
            context.Device.OperatingSystem = Environment.OSVersion.ToString();

            string screenResolutionData = GetScreenResolutionData();
            context.Device.ScreenResolution = screenResolutionData;

            context.Properties.Add("64BitOS", Environment.Is64BitOperatingSystem.ToString());
            context.Properties.Add("64BitProcess", Environment.Is64BitProcess.ToString());
            context.Properties.Add("MachineName", Environment.MachineName);
            context.Properties.Add("ProcessorCount", Environment.ProcessorCount.ToString());
            context.Properties.Add("ClrVersion", Environment.Version.ToString());

            context.Session.Id = DateTime.Now.ToFileTime().ToString();
            context.Session.IsFirst = true;

            context.User.AccountId = Environment.UserDomainName;
            context.User.Id = Environment.UserName;
        }

        private string GetScreenResolutionData()
        {
            if (Screen.AllScreens.Length > 1)
            {
                System.Text.StringBuilder screenData = new System.Text.StringBuilder();
                for (int i = 0; i < Screen.AllScreens.Length; i++)
                {
                    var screen = Screen.AllScreens[i];
                    screenData.AppendLine(string.Format("[{0}] {1}x{2}", i, screen.Bounds.Width, screen.Bounds.Height));
                }
                return screenData.ToString();
            }
            else
            {
                return string.Concat(Screen.PrimaryScreen.Bounds.Width, "x", Screen.PrimaryScreen.Bounds.Height);
            }
        }
    }
}
