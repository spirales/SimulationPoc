using GMap.NET;
using GMap.NET.WindowsPresentation;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.Logging;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;
namespace ActorSensor.ReceiverDisplay;
public class MainViewModel : INotifyPropertyChanged
{
    private bool _isMapUpdatePaused;
    public HubConnection? _hubConnection;

    public event PropertyChangedEventHandler? PropertyChanged;

    // Observable collection for markers to be bound to the View
    private Dictionary<string, GMapMarker> _actorMarkers = new Dictionary<string, GMapMarker>();
    private Dictionary<string, ActorInfo> _actorData = new Dictionary<string, ActorInfo>(); // To store actor info

    public PointLatLng MapCenter { get; set; } = new PointLatLng(45.9432, 24.9668); // Romania center
    public double MapZoom { get; set; } = 4;

    public ICommand ToggleMapUpdateCommand { get; set; }
    GMapControl MapControl;
    public MainViewModel(GMapControl MapControl)
    {
        this.MapControl = MapControl;
        ToggleMapUpdateCommand = new RelayCommand(ToggleMapUpdate);
         InitializeSignalR();
    }

    private async void InitializeSignalR()
    {
        _hubConnection = new HubConnectionBuilder()
            .WithUrl("http://localhost:7001/realtime/positionupdates")
            .Build();

        _hubConnection.On<string, double, double, DateTime>("ReceivePositionUpdate", (actorId, latitude, longitude, timeStamp) =>
        {
            if (!_isMapUpdatePaused)
            {
                Application.Current.Dispatcher.Invoke(() =>
               {
                   UpdateActorPosition(actorId, latitude, longitude, timeStamp, timeStamp.ToString("o"));
               });
            }
        });

        try
        {
            await _hubConnection.StartAsync();
        }
        catch
        {
        }
    }

    private void UpdateActorPosition(string actorId, double latitude, double longitude, DateTime timeStamp, string additionalInfo)
    {
        // Create or update the ActorInfo object
        var actorInfo = new ActorInfo
        {
            ActorId = actorId,
            Latitude = latitude,
            Longitude = longitude,
            LastUpdated = timeStamp,
            AdditionalInfo = additionalInfo
        };

        _actorData[actorId] = actorInfo; // Store actor info

        // Check if the actor already has a marker on the map
        if (_actorMarkers.ContainsKey(actorId))
        {
            // Update the actor's marker position
            var marker = _actorMarkers[actorId];
            marker.Position = new PointLatLng(latitude, longitude);

            // Update the tooltip content dynamically
            ToolTipService.SetToolTip(marker.Shape, CreateToolTipContent(actorInfo));
        }
        else
        {
            // Create a new marker for the actor
            var ellipse = new System.Windows.Shapes.Ellipse
            {
                Width = 20,
                Height = 20,
                Stroke = System.Windows.Media.Brushes.Red,
                StrokeThickness = 1.5,
                Fill = System.Windows.Media.Brushes.Red
            };

            // Set the tooltip for the marker shape
            ToolTipService.SetToolTip(ellipse, CreateToolTipContent(actorInfo));

            var marker = new GMapMarker(new PointLatLng(latitude, longitude))
            {
                Shape = ellipse
            };

            // Add the marker to the map
            MapControl.Markers.Add(marker);

            // Store the marker in the dictionary
            _actorMarkers[actorId] = marker;
        }
    }
    private object CreateToolTipContent(ActorInfo actorInfo)
    {
        // Create a StackPanel to hold multiple lines of information
        var toolTipPanel = new StackPanel();

        // Add each piece of information as a TextBlock
        toolTipPanel.Children.Add(new TextBlock { Text = $"Actor ID: {actorInfo.ActorId}" });
        toolTipPanel.Children.Add(new TextBlock { Text = $"Latitude: {actorInfo.Latitude}" });
        toolTipPanel.Children.Add(new TextBlock { Text = $"Longitude: {actorInfo.Longitude}" });
        toolTipPanel.Children.Add(new TextBlock { Text = $"Last Updated: {actorInfo.LastUpdated}" });
        toolTipPanel.Children.Add(new TextBlock { Text = $"Info: {actorInfo.AdditionalInfo}" });

        return toolTipPanel;
    }

    private void ToggleMapUpdate()
    {
        _isMapUpdatePaused = !_isMapUpdatePaused;
    }

    protected virtual void OnPropertyChanged(string propertyName)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
