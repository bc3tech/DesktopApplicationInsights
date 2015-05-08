
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Microsoft.ApplicationInsights.DataContracts;

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
        /// Raises the <see cref="E:Click" /> event.
        /// </summary>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected override void OnClick(EventArgs e)
        {
            var startTime = DateTime.UtcNow;

            base.OnClick(e);

            if (!string.IsNullOrWhiteSpace(this.EventName))
            {
                var telemetryData = new EventTelemetry(this.EventName);
                telemetryData.Timestamp = startTime;
                if (this.IsTimed)
                {
                    telemetryData.Metrics.Add("Duration", (DateTime.UtcNow - startTime).TotalMilliseconds);
                }
                Telemetry.Client.TrackEvent(telemetryData);
            }
        }
    }
}
