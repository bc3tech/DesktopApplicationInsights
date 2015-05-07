using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading;
using Microsoft.ApplicationInsights.DataContracts;

namespace DesktopApplicationInsights
{
    public class TimedTelemetryEvent : IDisposable
    {
        private readonly DateTime _startTime;
        private readonly string _eventName;
        private readonly IDictionary<string, string> _properties;
        private readonly IDictionary<string, double> _metrics;

        public TimedTelemetryEvent(string eventName, IDictionary<string, string> properties = null,
            IDictionary<string, double> metrics = null)
        {
            _startTime = DateTime.UtcNow;
            _eventName = eventName;
            _properties = properties ?? new Dictionary<string, string>();
            _metrics = metrics ?? new Dictionary<string, double>();
        }

        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    var eventTelemetry = new EventTelemetry(_eventName)
                    {
                        Timestamp = _startTime,
                    };

                    _properties.ToList().ForEach(p => eventTelemetry.Properties.Add(p));

                    _metrics.ToList().ForEach(m => eventTelemetry.Metrics.Add(m));
                    eventTelemetry.Metrics.Add(string.Concat(_eventName, "_Duration"),
                        (DateTime.UtcNow - _startTime).TotalMilliseconds);

                    Telemetry.Client.TrackEvent(eventTelemetry);
                }

                // TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
                // TODO: set large fields to null.

                disposedValue = true;
            }
        }

        // TODO: override a finalizer only if Dispose(bool disposing) above has code to free unmanaged resources.
        // ~TimedTelemetryEvent() {
        //   // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
        //   Dispose(false);
        // }

        // This code added to correctly implement the disposable pattern.
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
            // TODO: uncomment the following line if the finalizer is overridden above.
            // GC.SuppressFinalize(this);
        }
        #endregion
    }
}