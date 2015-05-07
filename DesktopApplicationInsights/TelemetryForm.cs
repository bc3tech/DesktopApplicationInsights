using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.ApplicationInsights.Extensibility;
using System;
using System.Windows.Forms;

namespace DesktopApplicationInsights
{
    public partial class TelemetryForm : Form
    {
        private readonly PageViewTelemetry _telemetryData;
        private bool _viewLogged = false;
        private DateTime _openTime = DateTime.UtcNow;
        public TelemetryForm()
        {
            _telemetryData = new PageViewTelemetry(
                !string.IsNullOrWhiteSpace(this.Name) ? this.Name : this.GetType().Name);
        }

        protected override void OnLoad(EventArgs e)
        {
            _openTime = DateTime.UtcNow;

            base.OnLoad(e);
        }

        protected override void OnClosed(EventArgs e)
        {
            LogPageView();

            base.OnClosed(e);
        }

        private void LogPageView()
        {
            _telemetryData.Duration = DateTime.UtcNow - _openTime;
            Telemetry.Client.TrackPageView(_telemetryData);
            _viewLogged = true;
        }
    }
}
