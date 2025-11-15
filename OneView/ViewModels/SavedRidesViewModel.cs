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
    public partial class SavedRidesViewModel : ObservableObject
    {
        [ObservableProperty]
        private ObservableCollection<RideHistoryItem> rides = new();

        [ObservableProperty]
        private bool hasRides = false;

        [ObservableProperty]
        private RideHistoryItem? selectedRide;

        public SavedRidesViewModel()
        {
            LoadRides();
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

                // Load the most recent ride profile
                var profile = App.ProfileService.LoadRideData();

                if (profile != null && profile.Distance > 0)
                {
                    var rideItem = new RideHistoryItem
                    {
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
                Debug.WriteLine($" Loaded {Rides.Count} ride(s)");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($" Error loading rides: {ex.Message}");
            }
        }

        /// <summary>
        /// Shows details of the selected ride
        /// </summary>
        [RelayCommand]
        private async Task ViewRideDetails(RideHistoryItem ride)
        {
            if (ride == null) return;

            await Shell.Current.DisplayAlert(
                $"Fahrt vom {ride.Date:dd.MM.yyyy}",
                $" Distanz: {ride.Distance:F2} km\n" +
                $" Dauer: {ride.Duration:hh\\:mm\\:ss}\n" +
                $" Durchschn.: {ride.AverageSpeed:F1} km/h\n" +
                $" Max: {ride.MaxSpeed:F1} km/h\n\n" +
                $"Neigung Links: {ride.MinInclineLeft:F1}° - {ride.MaxInclineLeft:F1}°\n" +
                $"Neigung Rechts: {ride.MinInclineRight:F1}° - {ride.MaxInclineRight:F1}°",
                "OK");
        }

        /// <summary>
        /// Clears all saved rides
        /// </summary>
        [RelayCommand]
        private async Task ClearAllRides()
        {
            bool confirm = await Shell.Current.DisplayAlert(
                "Löschen bestätigen",
                "Möchten Sie alle gespeicherten Fahrten löschen?",
                "Ja",
                "Nein");

            if (confirm)
            {
                App.ProfileService.ClearRideData();
                var saveService = new Services.SaveprofileData();
                saveService.ClearAllData();
                
                LoadRides();
                
                await Shell.Current.DisplayAlert("Gelöscht", "Alle Fahrten wurden gelöscht.", "OK");
            }
        }
    }

    /// <summary>
    /// Represents a single ride in the history list
    /// </summary>
    public class RideHistoryItem
    {
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
        public string DisplayAvgSpeed => $"Ø {AverageSpeed:F1} km/h";
        public string DisplayMaxSpeed => $"Max {MaxSpeed:F1} km/h";
    }
}
