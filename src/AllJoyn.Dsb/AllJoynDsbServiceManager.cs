using AllJoyn.Dsb;
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

namespace AllJoyn.Dsb
{
    public sealed class AllJoynDsbServiceManager
    {
        private static AllJoynDsbServiceManager instance;
        private Adapter adapter;
        private DsbBridge dsbBridge;
        private Task m_startupTask;


        public static AllJoynDsbServiceManager Current
        {
            get
            {
                if (instance == null)
                    instance = new AllJoynDsbServiceManager();
                return instance;
            }
        }

        public ServiceState State { get; private set; } = ServiceState.Stopped;

        public enum ServiceState
        {
            Stopped,
            Starting,
            Running,
            Stopping
        }

        public async Task ShutdownAsync()
        {
            State = ServiceState.Stopping;
            foreach (var device in Devices.ToArray())
                RemoveDevice(device);
            await Task.Delay(1000); //Give it some time to announce devices lost
            m_startupTask = null;
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
            
        }
        
        public Task StartAsync(BridgeConfiguration configuration)
        {
            if (configuration == null)
                throw new ArgumentNullException(nameof(configuration));
            if (m_startupTask == null)
            {
                m_startupTask = ThreadPool.RunAsync(new WorkItemHandler((IAsyncAction action) =>
                {
                    State = ServiceState.Starting;
                    try
                    {
                        adapter = new Adapter(configuration);
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
            return m_startupTask;
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
