using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using OneView.Models;
using System.Collections.ObjectModel;
using System.Diagnostics;

namespace OneView.ViewModels
{
    /// <summary>
    /// ViewModel for the Saved Rides page
    /// Displays ride history and allows viewing ride details
    /// </summary>
    public partial class SavedRidesViewModel : ObservableObject, IDisposable
    {
        private System.Timers.Timer? _rideTimer;

        #region Observable Properties

        [ObservableProperty]
        private ObservableCollection<RideHistoryItem> rides = new();

        [ObservableProperty]
        private bool hasRides = false;

        [ObservableProperty]
        private bool isRideActive = false;

        [ObservableProperty]
        private int? expandedRideId = null;

        // Live data properties
        [ObservableProperty]
        private string liveDistance = "0.00";

        [ObservableProperty]
        private string liveDuration = "00:00:00";

        [ObservableProperty]
        private string liveAvgSpeed = "0.0";

        [ObservableProperty]
        private string liveMinInclineLeft = "0.0";

        [ObservableProperty]
        private string liveMaxInclineLeft = "0.0";

        [ObservableProperty]
        private string liveMinInclineRight = "0.0";

        [ObservableProperty]
        private string liveMaxInclineRight = "0.0";

        #endregion

        public SavedRidesViewModel()
        {
            LoadRides();
            IsRideActive = App.ProfileService?.IsRideActive ?? false;

            if (IsRideActive)
            {
                StartRideTimer();
            }

            Debug.WriteLine("? SavedRidesViewModel initialized");
        }

        /// <summary>
        /// Loads ride history from saved data
        /// </summary>
        [RelayCommand]
        private void LoadRides()
        {
            try
            {
                Rides.Clear();

                var profile = App.ProfileService.LoadRideData();

                if (profile != null && profile.Distance > 0)
                {
                    var rideItem = new RideHistoryItem
                    {
                        Id = 1,
                        Date = profile.LastTime,
                        Distance = profile.Distance,
                        Duration = profile.TimeOnBike,
                        AverageSpeed = profile.MediumSpeed,
                        MaxSpeed = profile.MaxSpeed,
                        MinInclineLeft = profile.MinInclineAngleLeft,
                        MaxInclineLeft = profile.MaxInclineAngleLeft,
                        MinInclineRight = profile.MinInclineAngleRight,
                        MaxInclineRight = profile.MaxInclineAngleRight
                    };

                    Rides.Add(rideItem);
                }

                HasRides = Rides.Count > 0;
                Debug.WriteLine($"?? Loaded {Rides.Count} ride(s)");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"? Error loading rides: {ex.Message}");
            }
        }

        /// <summary>
        /// Toggles the expanded state of a ride item
        /// </summary>
        [RelayCommand]
        private void ToggleRideDetails(RideHistoryItem ride)
        {
            if (ExpandedRideId == ride.Id)
            {
                ExpandedRideId = null;
            }
            else
            {
                ExpandedRideId = ride.Id;
            }
        }

        /// <summary>
        /// Starts a new ride tracking session
        /// </summary>
        [RelayCommand]
        private void StartRide()
        {
            App.ProfileService?.StartRide();
            IsRideActive = true;
            StartRideTimer();
            Debug.WriteLine("?? Ride started from SavedRidesViewModel");
        }

        /// <summary>
        /// Stops the current ride and saves data
        /// </summary>
        [RelayCommand]
        private void StopRide()
        {
            App.ProfileService?.StopRide();
            IsRideActive = false;
            StopRideTimer();
            LoadRides();
            Debug.WriteLine("?? Ride stopped from SavedRidesViewModel");
        }

        /// <summary>
        /// Starts the timer for live data updates
        /// </summary>
        private void StartRideTimer()
        {
            _rideTimer = new System.Timers.Timer(1000);
            _rideTimer.Elapsed += (sender, e) => UpdateLiveStats();
            _rideTimer.Start();
            Debug.WriteLine("?? Live stats timer started");
        }

        /// <summary>
        /// Stops the live data timer
        /// </summary>
        private void StopRideTimer()
        {
            _rideTimer?.Stop();
            _rideTimer?.Dispose();
            _rideTimer = null;
            Debug.WriteLine("?? Live stats timer stopped");
        }

        /// <summary>
        /// Updates live statistics from the current ride profile
        /// </summary>
        private void UpdateLiveStats()
        {
            try
            {
                var profile = App.ProfileService?.CurrentRideProfile;
                if (profile != null)
                {
                    LiveDistance = profile.Distance.ToString("F2");
                    LiveDuration = profile.TimeOnBike.ToString(@"hh\:mm\:ss");
                    LiveAvgSpeed = profile.MediumSpeed.ToString("F1");
                    LiveMinInclineLeft = profile.MinInclineAngleLeft.ToString("F1");
                    LiveMaxInclineLeft = profile.MaxInclineAngleLeft.ToString("F1");
                    LiveMinInclineRight = profile.MinInclineAngleRight.ToString("F1");
                    LiveMaxInclineRight = profile.MaxInclineAngleRight.ToString("F1");

                    Debug.WriteLine($"?? Live stats updated: Distance={LiveDistance}km, Duration={LiveDuration}");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"? Error updating live stats: {ex.Message}");
            }
        }

        /// <summary>
        /// Clears all saved rides after confirmation
        /// </summary>
        [RelayCommand]
        private async Task ClearAllRides()
        {
            bool confirm = await Shell.Current.DisplayAlertAsync(
                "Löschen bestätigen",
                "Möchten Sie alle gespeicherten Fahrten löschen?",
                "Ja",
                "Nein");

            if (confirm)
            {
                App.ProfileService?.ClearRideData();
                var saveService = new Services.SaveprofileData();
                saveService.ClearAllData();

                LoadRides();
                ExpandedRideId = null;

                Debug.WriteLine("??? All rides cleared");

                await Shell.Current.DisplayAlertAsync("Gelöscht", "Alle Fahrten wurden gelöscht.", "OK");
            }
        }

        public void Dispose()
        {
            StopRideTimer();
            Debug.WriteLine("?? SavedRidesViewModel disposed");
        }
    }

    /// <summary>
    /// Represents a single ride in the history list
    /// </summary>
    public class RideHistoryItem
    {
        public int Id { get; set; }
        public DateTime Date { get; set; }
        public double Distance { get; set; }
        public TimeSpan Duration { get; set; }
        public double AverageSpeed { get; set; }
        public double MaxSpeed { get; set; }
        public double MinInclineLeft { get; set; }
        public double MaxInclineLeft { get; set; }
        public double MinInclineRight { get; set; }
        public double MaxInclineRight { get; set; }

        public string DisplayDate => Date.ToString("dd.MM.yyyy HH:mm");
        public string DisplayDistance => $"{Distance:F2} km";
        public string DisplayDuration => Duration.ToString(@"hh\:mm\:ss");
        public string DisplayAvgSpeed => $"? {AverageSpeed:F1} km/h";
    }
}
