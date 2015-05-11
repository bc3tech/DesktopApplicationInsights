
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.ApplicationInsights.Extensibility;

namespace DesktopApplicationInsights
{
    /// <summary>A <c>Button</c> class that automatically logs telemetry data when clicked</summary>
    [DesignTimeVisible]
    public class TelemetryButton : Button
    {
        /// <summary>
        /// Gets or sets the Event Name to use when logging the Button's execution
        /// </summary>
        [EditorBrowsable]
        public string EventName { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is timed. <c>true</c> to attach execution 
        /// duration to the logged event. <c>false</c> otherwise.
        /// </summary>
        [EditorBrowsable]
        public bool IsTimed { get; set; }

        /// <summary>
        /// Gets or sets the name of the telemetry client used by the button
        /// </summary>
        [EditorBrowsable]
        public string TelemetryClientName { get; set; }

        private TelemetryClient _telemetryClient;
        /// <summary>
        /// Called when [create control].
        /// </summary>
        protected override void OnCreateControl()
        {
            if (!this.DesignMode)
            {
                System.Diagnostics.Debug.Assert(!string.IsNullOrWhiteSpace(this.TelemetryClientName),
                    string.Format("No Telemetry client name set on Telemetry Button \"{0}\"", this.Name));

                if (!string.IsNullOrWhiteSpace(this.TelemetryClientName))
                {
                    try
                    {
                        _telemetryClient = Telemetry.GetClient(this.TelemetryClientName);
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.Fail(string.Format("Couldn't find telemetry client with name {0}",
                            this.TelemetryClientName), ex.ToString());
                    }
                }
            }

            base.OnCreateControl();
        }

        /// <summary>
        /// Raises the <see cref="E:Click" /> event.
        /// </summary>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected override void OnClick(EventArgs e)
        {
            if (_telemetryClient != null)
            {
                var startTime = DateTime.UtcNow;
                base.OnClick(e);

                if (!string.IsNullOrWhiteSpace(this.EventName))
                {
                    var telemetryData = new EventTelemetry(this.EventName);
                    telemetryData.Timestamp = startTime;
                    if (this.IsTimed)
                    {
                        telemetryData.Metrics.Add(string.Concat(this.EventName, "_Duration"),
                            (DateTime.UtcNow - startTime).TotalMilliseconds);
                    }
                    _telemetryClient.TrackEvent(telemetryData);
                }
            }
        }
    }
}
