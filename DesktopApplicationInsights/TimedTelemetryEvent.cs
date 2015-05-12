using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.DataContracts;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DesktopApplicationInsights
{
    /// <summary>
    /// Provides duration information for tracked event by utilizing the <c>IDisposable</c> pattern
    /// </summary>
    public class TimedTelemetryEvent : IDisposable
    {
        private readonly DateTime _startTime;
        private readonly string _eventName;
        private readonly TelemetryClient _client;

        /// <summary>Gets the properties.</summary>
        public IDictionary<string, string> Properties { get; private set; }

        /// <summary>Gets the metrics.</summary>
        public IDictionary<string, double> Metrics { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="TimedTelemetryEvent" /> class.
        /// </summary>
        /// <param name="client">The telemetry client to use to log the timed event(s).</param>
        /// <param name="eventName">Name of the event.</param>
        /// <param name="properties">Properties to tie to the event</param>
        /// <param name="metrics">Metrics to tie to the event</param>
        public TimedTelemetryEvent(TelemetryClient client, string eventName,
            IDictionary<string, string> properties = null,
            IDictionary<string, double> metrics = null)
        {
            _startTime = DateTime.UtcNow;
            _eventName = eventName;
            this.Properties = properties ?? new Dictionary<string, string>();
            this.Metrics = metrics ?? new Dictionary<string, double>();
            _client = client;
        }

        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        /// <summary>
        /// Releases unmanaged and - optionally - managed resources.
        /// </summary>
        /// <param name="disposing"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
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

                    this.Properties.ToList().ForEach(p => eventTelemetry.Properties.Add(p));

                    this.Metrics.ToList().ForEach(m => eventTelemetry.Metrics.Add(m));
                    eventTelemetry.Metrics.Add(string.Concat(_eventName, "_Duration"),
                        (DateTime.UtcNow - _startTime).TotalMilliseconds);

                    _client.TrackEvent(eventTelemetry);
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
        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
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