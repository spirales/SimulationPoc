using System.Windows;
using GMap.NET;
using GMap.NET.WindowsPresentation;
using GMap.NET.MapProviders;
using System.Windows.Controls;

namespace ActorSensor.ReceiverDisplay;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        SetupMap();
        DataContext = new MainViewModel(MapControl);
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
}
