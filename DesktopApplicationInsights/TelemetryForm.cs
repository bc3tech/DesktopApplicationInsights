using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.ApplicationInsights.Extensibility;
using System;
using System.Windows.Forms;
using Microsoft.ApplicationInsights;

namespace DesktopApplicationInsights
{
    /// <summary>
    /// Base class for forms who wish to track Telemetry data as a PageView event.
    /// Also applies duration to the logged entry
    /// </summary>
    public abstract class TelemetryForm : Form
    {
        private readonly PageViewTelemetry _telemetryData;
        private bool _viewLogged = false;
        private DateTime _openTime = DateTime.UtcNow;
        /// <summary>
        /// Initializes a new instance of the <see cref="TelemetryForm"/> class.
        /// </summary>
        public TelemetryForm()
        {
            _telemetryData = new PageViewTelemetry(
                !string.IsNullOrWhiteSpace(this.Name) ? this.Name : this.GetType().Name);
        }

        /// <summary>
        /// Gets the telemetry client used by this form for automatically logging PageView events
        /// </summary>
        protected abstract TelemetryClient TelemetryClient { get; }

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
