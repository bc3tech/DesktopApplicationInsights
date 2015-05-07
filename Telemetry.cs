using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.ApplicationInsights.Extensibility;
using System;
using System.Globalization;
using System.Windows.Forms;

namespace MyTelemetryClient
{
    public class Telemetry
    {
        public static TelemetryClient Client { get; }

        static Telemetry()
        {
            Client = new TelemetryClient();
        }

        public static void ConfigureApplication(string instrumentationKey)
        {
            TelemetryConfiguration.Active.ContextInitializers.Add(new TelemetryInitializer());

            Application.SetUnhandledExceptionMode(UnhandledExceptionMode.CatchException);
            Application.ThreadException += (s, e) =>
            {
                Telemetry.Client.TrackException(e.Exception);
            };
        }

        ~Telemetry()
        {
            if (Client != null)
            {
                Client.Flush();
            }
        }
    }

    class TelemetryInitializer : IContextInitializer
    {
        public void Initialize(TelemetryContext context)
        {
            context.Component.Version = Application.ProductVersion;

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
