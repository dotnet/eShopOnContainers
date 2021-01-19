using eShopOnContainers.Core.Models.Location;
using eShopOnContainers.Core.Services.Location;
using eShopOnContainers.Windows.Services;
using System;
using System.Threading;
using System.Threading.Tasks;
using Windows.Devices.Geolocation;
using Windows.Foundation;

[assembly: Xamarin.Forms.Dependency(typeof(LocationServiceImplementation))]
namespace eShopOnContainers.Windows.Services
{
    public class LocationServiceImplementation : ILocationServiceImplementation
    {
        double _desiredAccuracy;
        Geolocator _locator = new Geolocator();

        public LocationServiceImplementation()
        {
            DesiredAccuracy = 100;
        }

        #region Internal Implementation

        static Position GetPosition(Geoposition position)
        {
            var pos = new Position
            {
                Accuracy = position.Coordinate.Accuracy,
                Altitude = position.Coordinate.Point.Position.Altitude,
                Latitude = position.Coordinate.Point.Position.Latitude,
                Longitude = position.Coordinate.Point.Position.Longitude,
                Timestamp = position.Coordinate.Timestamp.ToUniversalTime()
            };

            if (position.Coordinate.Heading != null)
                pos.Heading = position.Coordinate.Heading.Value;
            if (position.Coordinate.Speed != null)
                pos.Speed = position.Coordinate.Speed.Value;
            if (position.Coordinate.AltitudeAccuracy.HasValue)
                pos.AltitudeAccuracy = position.Coordinate.AltitudeAccuracy.Value;

            return pos;
        }

        #endregion

        #region ILocationServiceImplementation

        public double DesiredAccuracy
        {
            get { return _desiredAccuracy; }
            set
            {
                _desiredAccuracy = value;
                _locator.DesiredAccuracy = (value < 100) ? PositionAccuracy.High : PositionAccuracy.Default;
            }
        }

        public bool IsGeolocationAvailable
        {
            get
            {
                var status = _locator.LocationStatus;
                while (status == PositionStatus.Initializing)
                {
                    status = _locator.LocationStatus;
                }
                return status != PositionStatus.NotAvailable;
            }
        }

        public bool IsGeolocationEnabled
        {
            get
            {
                var status = _locator.LocationStatus;
                while (status == PositionStatus.Initializing)
                {
                    status = _locator.LocationStatus;
                }
                return status != PositionStatus.Disabled && status != PositionStatus.NotAvailable;
            }
        }

        public Task<Position> GetPositionAsync(TimeSpan? timeout = null, CancellationToken? cancelToken = null)
        {
            var timeoutMilliseconds = timeout.HasValue ? (int)timeout.Value.TotalMilliseconds : eShopOnContainers.Windows.Helpers.Timeout.Infinite;
            if (timeoutMilliseconds < 0 && timeoutMilliseconds != eShopOnContainers.Windows.Helpers.Timeout.Infinite)
                throw new ArgumentOutOfRangeException(nameof(timeout));

            if (!cancelToken.HasValue)
                cancelToken = CancellationToken.None;

            var pos = _locator.GetGeopositionAsync(TimeSpan.FromTicks(0), TimeSpan.FromDays(365));
            cancelToken.Value.Register(o => ((IAsyncOperation<Geoposition>)o).Cancel(), pos);
            var timer = new eShopOnContainers.Windows.Helpers.Timeout(timeoutMilliseconds, pos.Cancel);
            var tcs = new TaskCompletionSource<Position>();

            pos.Completed = (op, s) =>
            {
                timer.Cancel();
                switch (s)
                {
                    case AsyncStatus.Canceled:
                        tcs.SetCanceled();
                        break;
                    case AsyncStatus.Completed:
                        tcs.SetResult(GetPosition(op.GetResults()));
                        break;
                    case AsyncStatus.Error:
                        var ex = op.ErrorCode;
                        if (ex is UnauthorizedAccessException)
                            ex = new GeolocationException(GeolocationError.Unauthorized, ex);

                        tcs.SetException(ex);
                        break;
                }
            };
            return tcs.Task;           
        }

        #endregion
    }
}
