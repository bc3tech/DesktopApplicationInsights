using Microsoft.ApplicationInsights.DataContracts;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MyTelemetryClient
{
    public partial class TelemetryForm : Form
    {
        private readonly PageViewTelemetry _telemetryData;
        private bool _viewLogged = false;
        private DateTime _openTime = DateTime.Now;
        public TelemetryForm()
        {
            _telemetryData = new PageViewTelemetry(this.Name);

            InitializeComponent();
        }

        protected override void OnLoad(EventArgs e)
        {
            _openTime = DateTime.Now;

            base.OnLoad(e);
        }

        protected override void OnClosed(EventArgs e)
        {
            LogPageView();

            base.OnClosed(e);
        }

        private void LogPageView()
        {
            _telemetryData.Duration = DateTime.Now - _openTime;
            Telemetry.Client.TrackPageView(_telemetryData);
            _viewLogged = true;
        }
    }
}
