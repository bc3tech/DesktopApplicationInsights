using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.ApplicationInsights.Extensibility;
using System;
using System.Windows.Forms;
using Microsoft.ApplicationInsights;
using System.ComponentModel;

namespace DesktopApplicationInsights
{
    /// <summary>
    /// Base class for forms who wish to track Telemetry data as a PageView event.
    /// Also applies duration to the logged entry
    /// </summary>
    public class TelemetryForm : Form
    {
        private readonly PageViewTelemetry _telemetryData;
        private readonly Lazy<TelemetryClient> _telemetryClientFetcher;

        private bool _viewLogged = false;
        private DateTime _openTime = DateTime.UtcNow;

        /// <summary>
        /// Initializes a new instance of the <see cref="TelemetryForm"/> class.
        /// </summary>
        public TelemetryForm()
        {
            _telemetryData = new PageViewTelemetry(
                !string.IsNullOrWhiteSpace(this.Name) ? this.Name : this.GetType().Name);

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

        /// <summary>
        /// Raises the <see cref="E:Load" /> event.
        /// </summary>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected override void OnLoad(EventArgs e)
        {
            _openTime = DateTime.UtcNow;

            base.OnLoad(e);
        }

        /// <summary>
        /// Raises the <see cref="E:Closed" /> event.
        /// </summary>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected override void OnClosed(EventArgs e)
        {
            LogPageView();

            base.OnClosed(e);
        }

        private void LogPageView()
        {
            if (!_viewLogged)
            {
                _telemetryData.Duration = DateTime.UtcNow - _openTime;
                this.TelemetryClient.TrackPageView(_telemetryData);
                _viewLogged = true;
            }
        }

        /// <summary>
        /// Releases unmanaged and - optionally - managed resources.
        /// </summary>
        /// <param name="disposing"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
        protected override void Dispose(bool disposing)
        {
            LogPageView();

            base.Dispose(disposing);
        }
    }
}
