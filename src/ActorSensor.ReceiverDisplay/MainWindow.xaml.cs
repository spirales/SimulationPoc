using Microsoft.AspNetCore.SignalR.Client;
using System.Windows;
using GMap.NET;
using GMap.NET.WindowsPresentation;
using GMap.NET.MapProviders;
using System.Windows.Controls;

namespace ActorSensor.ReceiverDisplay;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    private Dictionary<string, GMapMarker> _actorMarkers = new Dictionary<string, GMapMarker>();
    private Dictionary<string, ActorInfo> _actorData = new Dictionary<string, ActorInfo>(); // To store actor info
    public required HubConnection _hubConnection;
    private CancellationTokenSource _cts = new CancellationTokenSource();
    public MainWindow()
    {
        InitializeComponent();
        InitializeSignalR();
        SetupMap();
    }
    private async void InitializeSignalR()
    {
        _hubConnection = new HubConnectionBuilder()
            .WithUrl("http://localhost:7001/realtime/positionupdates")
            .Build();

        _hubConnection.On<string, double, double, DateTime>("ReceivePositionUpdate",
            (actorId, latitude, longitude, timeStamp) =>
            {
                Dispatcher.Invoke(() =>
                {
                    UpdateActorPosition(actorId, latitude, longitude, timeStamp, timeStamp.ToString("o"));
                });
            });

        try
        {
            if (_hubConnection.State == HubConnectionState.Disconnected)
            {
                await _hubConnection.StartAsync();
            }

            MessageBox.Show("Connected to SignalR Hub");
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error connecting to SignalR: {ex.Message}");
        }
    }

    private void SetupMap()
    {
        MapControl.MapProvider = GMapProviders.GoogleMap;
        GMaps.Instance.Mode = AccessMode.ServerAndCache;

        // Set the initial map position (center) and zoom level
        MapControl.Position = new PointLatLng(45.9432, 24.9668); // Example: center of Romania
        MapControl.MinZoom = 2;
        MapControl.MaxZoom = 18;
        MapControl.Zoom = 12; // Set an appropriate zoom level based on your needs

        // Disable dragging and zooming to keep the map fixed
        MapControl.CanDragMap = false;
        MapControl.MouseWheelZoomType = MouseWheelZoomType.MousePositionWithoutCenter;
        MapControl.IgnoreMarkerOnMouseWheel = true;
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
                Width = 10,
                Height = 10,
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
}