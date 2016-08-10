using AdapterLib;
using AdapterLib.MockDevices;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace TestApp
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        public MainPage()
        {
            this.InitializeComponent();
            CheckBridgeStatus();
            App.Current.Suspending += Current_Suspending;
            App.Current.Resuming += Current_Resuming;
        }

        private async void Current_Suspending(object sender, Windows.ApplicationModel.SuspendingEventArgs e)
        {
            if (this.DataContext != null)
            {
                this.DataContext = null;
                var d = e.SuspendingOperation.GetDeferral();
                await AllJoynDsbServiceManager.Current.Shutdown();
                d.Complete();
            }
        }

        private void Current_Resuming(object sender, object e)
        {
            CheckBridgeStatus();
        }

        private async void CheckBridgeStatus()
        {
            status.Text = "Starting up bridge...";
            try
            {
                await AllJoynDsbServiceManager.Current.StartupTask;
                status.Text = "Bridge Running"; // Bridge Successfully Initialized
                LoadDevices();
            }
            catch (System.Exception ex)
            {
                status.Text = "Bridge failed to initialize:\n" + ex.Message;
            }
        }

        private void LoadDevices()
        {
            AllJoynDsbServiceManager.Current.AddDevice(new MockOnOffSwitchDevice("Mock Switch", Guid.NewGuid().ToString(), false));
            AllJoynDsbServiceManager.Current.AddDevice(new MockCurrentHumidityDevice("Mock Humidity", Guid.NewGuid().ToString(), 50));
            AllJoynDsbServiceManager.Current.AddDevice(new MockCurrentTemperatureDevice("Mock Temperature", Guid.NewGuid().ToString(), 25));
            AllJoynDsbServiceManager.Current.AddDevice(new MockBulbDevice(new MockLightingServiceHandler("Mock Light", Guid.NewGuid().ToString(), true, true, true, Dispatcher)));
        }
    }
}
