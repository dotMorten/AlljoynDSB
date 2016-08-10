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
                await AllJoynDeviceManager.Current.Shutdown();
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
                await AllJoynDeviceManager.Current.StartupTask;
                status.Text = ""; // Bridge Successfully Initialized
            }
            catch (System.Exception ex)
            {
                status.Text = "Bridge failed to initialize:\n" + ex.Message;
                return;
            }
            LoadDevices();
        }

        private void LoadDevices()
        {
            AllJoynDeviceManager.Current.AddDevice(new MockOnOffSwitchDevice("Mock Switch", Guid.NewGuid().ToString(), false));
        }
    }
}
