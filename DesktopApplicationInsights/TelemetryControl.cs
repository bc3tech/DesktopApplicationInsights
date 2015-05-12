using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Microsoft.ApplicationInsights;

namespace DesktopApplicationInsights
{
    /// <summary>
    /// Base Control class with built-in Telemetry wireup
    /// </summary>
    public class TelemetryControl : Control
    {
        private readonly Lazy<TelemetryClient> _telemetryClientFetcher;

        /// <summary>
        /// Initializes a new instance of the <see cref="TelemetryControl"/> class.
        /// </summary>
        public TelemetryControl() : base()
        {
            _telemetryClientFetcher = new Lazy<TelemetryClient>(() =>
            {
                System.Diagnostics.Debug.Assert(!string.IsNullOrWhiteSpace(this.TelemetryClientName),
                    string.Format("No Telemetry client name set on Telemetry Button \"{0}\"", this.Name));

                if (!string.IsNullOrWhiteSpace(this.TelemetryClientName))
                {
                    try
                    {
                        return Telemetry.GetClient(this.TelemetryClientName);
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.Fail(string.Format("Couldn't find telemetry client with name {0}",
                            this.TelemetryClientName), ex.ToString());
                    }
                }

                return null;
            });
        }

        /// <summary>
        /// Gets or sets the name of the telemetry client used by the button
        /// </summary>
        [EditorBrowsable]
        public string TelemetryClientName { get; set; }

        /// <summary>
        /// Gets the telemetry client.
        /// </summary>
        protected TelemetryClient TelemetryClient
        {
            get { return _telemetryClientFetcher.Value; }
        }
    }
}
