using AdapterLib;
using BridgeRT;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Devices.AllJoyn;
using Windows.Foundation;
using Windows.System.Threading;

namespace AdapterLib
{
    public sealed class AllJoynDsbServiceManager
    {
        private static AllJoynDsbServiceManager instance;
        private Adapter adapter;
        private DsbBridge dsbBridge;

        public static AllJoynDsbServiceManager Current
        {
            get
            {
                if (instance == null)
                    instance = new AllJoynDsbServiceManager();
                return instance;
            }
        }

        public Task StartupTask { get; private set; }

        public ServiceState State { get; private set; } = ServiceState.Stopped;

        public enum ServiceState
        {
            Stopped,
            Starting,
            Running,
            Stopping
        }

        public async Task Shutdown()
        {
            State = ServiceState.Stopping;
            foreach (var device in Devices.ToArray())
                RemoveDevice(device);
            await Task.Delay(1000); //Give it some time to announce devices lost
            StartupTask = null;
            dsbBridge.Shutdown();
            dsbBridge.Dispose();
            dsbBridge = null;
            adapter.Shutdown();
            adapter = null;
            instance = null;
            await Task.Delay(1000); //Give it some time to announce DSB lost
            State = ServiceState.Stopped;
        }

        private AllJoynDsbServiceManager()
        {
#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
            StartupTask = ThreadPool.RunAsync(new WorkItemHandler((IAsyncAction action) =>
            {
                State = ServiceState.Starting;
                try
                {
                    adapter = new Adapter();
                    dsbBridge = new DsbBridge(adapter);

                    var initResult = dsbBridge.Initialize();
                    if (initResult != 0)
                    {
                        throw new Exception("DSB Bridge initialization failed!");
                    }
                    State = ServiceState.Running;
                }
                catch (Exception)
                {
                    State = ServiceState.Stopped;
                    throw;
                }
            })).AsTask();
        }
        
        public void AddDevice(IAdapterDevice device)
        {
            if (State != ServiceState.Running)
                throw new InvalidOperationException("Service is not running");
            adapter.AddDevice((IAdapterDevice)device);
            _Devices.Add(device);
        }
        public void RemoveDevice(IAdapterDevice device)
        {
            adapter.RemoveDevice((IAdapterDevice)device);
            _Devices.Remove(device);
        }

        private ObservableCollection<IAdapterDevice> _Devices = new ObservableCollection<IAdapterDevice>();

        public IEnumerable<IAdapterDevice> Devices { get { return _Devices; } }
    }
}
