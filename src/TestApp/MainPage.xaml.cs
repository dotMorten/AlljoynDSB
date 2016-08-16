using AllJoyn.Dsb;
using AllJoyn.Dsb.MockDevices;
using System;
using Windows.UI.Xaml.Controls;


namespace TestApp
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        private class CustomAdapter : Adapter
        {
            public CustomAdapter(BridgeConfiguration config) : base(config)
            {
                AdapterBusObject abo = new AdapterBusObject("DSBConfig");
                var iface = new AdapterInterface("dotMorten.Configuration");
                iface.Properties.Add(new AdapterAttribute("TestProp", "String here", (o) =>
                {
                    string newValue = o as string;
                    return AllJoynStatusCode.Ok;
                }));
                iface.Methods.Add(new AdapterMethod("TestMethod", "A test method", (a, b, c) =>
                {
                }));
                iface.Signals.Add(new AdapterSignal("TestSignal"));
                Windows.UI.Xaml.DispatcherTimer timer = new Windows.UI.Xaml.DispatcherTimer() { Interval = TimeSpan.FromSeconds(2) };
                timer.Tick += Timer_Tick;
                timer.Start();
                abo.Interfaces.Add(iface);
                BusObjects.Add(abo);
            }

            private void Timer_Tick(object sender, object e)
            {
                NotifySignalListener(BusObjects[0].Interfaces[0].Signals[0]);
            }
        }
        public MainPage()
        {
            this.InitializeComponent();
            CheckBridgeStatus();
            App.Current.Suspending += Current_Suspending;
            App.Current.Resuming += Current_Resuming;
        }

        private async void CheckBridgeStatus()
        {
            status.Text = "Starting up bridge...";
            deviceList.ItemsSource = AllJoynDsbServiceManager.Current.Devices;
#if !DEBUG //Let things fail in debug
            try
            {
#endif
                var config = new BridgeConfiguration(GetDeviceID(), "com.dotMorten.TestApp")
                {
                    // The following are optional. If not set will be pulled from the package information and system information
                    ModelName = "TestApp Model", DeviceName = "TestApp DSB",
                    ApplicationName = "TestApp ApplicationName", Vendor = "MockDevices Inc"
                };
                await AllJoynDsbServiceManager.Current.StartAsync(new CustomAdapter(config));
                status.Text = "Bridge Running"; // Bridge Successfully Initialized
                LoadDevices();
#if !DEBUG





            catch (System.Exception ex)
            {
                status.Text = "Bridge failed to initialize:\n" + ex.Message;
            }
#endif
        }

        private void LoadDevices()
        {
            AllJoynDsbServiceManager.Current.AddDevice(new MockOnOffSwitchDevice("Mock Switch", Guid.NewGuid().ToString(), false));
            AllJoynDsbServiceManager.Current.AddDevice(new MockCurrentHumidityDevice("Mock Humidity", Guid.NewGuid().ToString(), 50));
            AllJoynDsbServiceManager.Current.AddDevice(new MockCurrentTemperatureDevice("Mock Temperature", Guid.NewGuid().ToString(), 25));
            AllJoynDsbServiceManager.Current.AddDevice(new MockBulbDevice(new MockLightingServiceHandler("Mock Light", Guid.NewGuid().ToString(), true, true, true, Dispatcher)));
        }

        private Guid GetDeviceID()
        {
            if(!Windows.Storage.ApplicationData.Current.LocalSettings.Values.ContainsKey("DSBDeviceId"))
            {
                Guid deviceId = Guid.NewGuid();
                Windows.Storage.ApplicationData.Current.LocalSettings.Values["DSBDeviceId"] = deviceId;
                return deviceId;
            }
            return (Guid)Windows.Storage.ApplicationData.Current.LocalSettings.Values["DSBDeviceId"];
        }


        #region Suspend/Resume: If you don't shut down the DSB during suspend, some clients could hand waiting for a response from the DSB
        private async void Current_Suspending(object sender, Windows.ApplicationModel.SuspendingEventArgs e)
        {
            if (this.DataContext != null)
            {
                this.DataContext = null;
                var d = e.SuspendingOperation.GetDeferral();
                await AllJoynDsbServiceManager.Current.ShutdownAsync();
                d.Complete();
            }
        }

        private void Current_Resuming(object sender, object e)
        {
            CheckBridgeStatus();
        }
        #endregion
    }
}
