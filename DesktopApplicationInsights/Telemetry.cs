using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.ApplicationInsights.Extensibility;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;

namespace DesktopApplicationInsights
{
    /// <summary>Root class providing easy access to <c>TelemetryClient</c> and global Telemetry Configuration</summary>
    public static class Telemetry
    {
        private static Dictionary<string, Tuple<TelemetryClient, TelemetryConfiguration>> _clientsAndConfigs = new Dictionary<string, Tuple<TelemetryClient, TelemetryConfiguration>>();
        /// <summary>
        /// Creates the client.
        /// </summary>
        /// <param name="clientName">Name of the client.</param>
        /// <param name="instrumentationKey">The instrumentation key.</param>
        /// <returns></returns>
        /// <exception cref="ArgumentException">
        /// A client already exists with name &lt;name&gt;;clientName
        /// or
        /// A client already exists with the given instrumentation key.;instrumentationKey
        /// </exception>
        public static TelemetryClient CreateClient(string clientName, string instrumentationKey)
        {
            return CreateClient(clientName, instrumentationKey, Assembly.GetCallingAssembly());
        }

        private static TelemetryClient CreateClient(string clientName, string instrumentationKey,
            Assembly sourceAssembly)
        {
            if (_clientsAndConfigs.ContainsKey(clientName))
            {
                throw new ArgumentException(
                    string.Format("A client already exists with name \"{0}\". Use GetClient() to retrieve it.", clientName),
                    "clientName");
            }

            if (_clientsAndConfigs.Any(c => c.Value.Item1.InstrumentationKey.Equals(instrumentationKey, StringComparison.OrdinalIgnoreCase)))
            {
                throw new ArgumentException(
                    "A client already exists with the given instrumentation key.", "instrumentationKey");
            }

            var config = TelemetryConfiguration.CreateDefault();
            var client = new TelemetryClient(config);
            ConfigureApplication(instrumentationKey, client, config,
                new TelemetryInitializer(sourceAssembly));

            _clientsAndConfigs.Add(clientName, Tuple.Create(client, config));

            return client;
        }

        /// <summary>
        /// Gets an existing Telemetry Client or creates a new one with the specified name &amp; instrumentation key
        /// </summary>
        /// <param name="clientName">Name of the client.</param>
        /// <param name="instrumentationKey">The instrumentation key.</param>
        /// <returns></returns>
        /// <exception cref="ArgumentException">
        /// A client already exists with the given instrumentation key.;instrumentationKey
        /// </exception>
        public static TelemetryClient GetOrCreateClient(string clientName, string instrumentationKey)
        {
            Tuple<TelemetryClient, TelemetryConfiguration> clientWithConfig;
            if (!_clientsAndConfigs.TryGetValue(clientName, out clientWithConfig))
            {
                CreateClient(clientName, instrumentationKey, Assembly.GetCallingAssembly());
            }
            return _clientsAndConfigs[clientName].Item1;
        }

        /// <summary>
        /// Gets the specified telemetry client.
        /// </summary>
        /// <param name="clientName">Name of the client.</param>
        /// <returns></returns>
        /// <exception cref="ArgumentException">No client has been crated with name &lt;name&gt;;clientName</exception>
        public static TelemetryClient GetClient(string clientName)
        {
            Tuple<TelemetryClient, TelemetryConfiguration> clientWithConfig;
            if (!_clientsAndConfigs.TryGetValue(clientName, out clientWithConfig))
            {
                throw new ArgumentException(string.Format("No client has been created with name \"{0}\"", clientName), "clientName");
            }

            return clientWithConfig.Item1;
        }

        private static void ConfigureApplication(string instrumentationKey,
            TelemetryClient client, TelemetryConfiguration config, TelemetryInitializer initializer)
        {
            config.InstrumentationKey = instrumentationKey;
            config.ContextInitializers.Add(initializer);

            Application.ThreadException += (s, e) =>
            {
                client.TrackException(new ExceptionTelemetry(e.Exception)
                {
                    HandledAt = ExceptionHandledAt.Unhandled,
                    SeverityLevel = SeverityLevel.Critical,
                });

                throw e.Exception;
            };

            Application.ApplicationExit += (s, e) =>
            {
                client.Flush();
            };
        }

        /// <summary>
        /// Helper method providing easy access to logging a *handled* exception during program execution
        /// </summary>
        /// <param name="client">The telemetry client to use.</param>
        /// <param name="ex">The exception that has been handled</param>
        /// <param name="severityLevel">The severity level to assign to the logged event</param>
        /// <param name="message">The message.</param>
        /// <param name="data">Any extra data desired to be associated with the exception's log entry</param>
        public static void LogHandledException(this TelemetryClient client, Exception ex, SeverityLevel severityLevel = SeverityLevel.Error,
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

            client.TrackException(o);
        }

        /// <summary>
        /// Disables the specified client.
        /// </summary>
        /// <param name="client">The client.</param>
        public static void Disable(this TelemetryClient client)
        {
            _clientsAndConfigs.Single(c => c.Value.Item1 == client).Value.Item2.DisableTelemetry = true;
        }

        /// <summary>
        /// Enables the specified client.
        /// </summary>
        /// <param name="client">The client.</param>
        public static void Enable(this TelemetryClient client)
        {
            _clientsAndConfigs.Single(c => c.Value.Item1 == client).Value.Item2.DisableTelemetry = false;
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
            context.Properties.Add("Machine name", Environment.MachineName);
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
