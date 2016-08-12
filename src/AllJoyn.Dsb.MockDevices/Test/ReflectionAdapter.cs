using AllJoyn.Dsb.MockDevices.Test;
using BridgeRT;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace AllJoyn.Dsb.MockDevices.Test
{
    public class ReflectionBusObject : AllJoyn.Dsb.AdapterBusObject
    {
        public ReflectionBusObject(object o, string objectPath) : base(objectPath)
        {
            var t = o.GetType().GetTypeInfo();
            foreach(var iface in t.ImplementedInterfaces)
            {
                if(iface.GetTypeInfo().IsSubclassOf(typeof(IAlljoynInterface)))
                {
                    var adapter = new ReflectionAdapter(o, iface);
                    Interfaces.Add(adapter);
                    adapter.PropertyChanged += Adapter_PropertyChanged;
                }
            }
        }

        private void Adapter_PropertyChanged(ReflectionAdapter sender, IAdapterAttribute args)
        {
            PropertyChanged.Invoke(this, new Tuple<ReflectionAdapter, IAdapterAttribute>(sender, args));
        }

        internal event Windows.Foundation.TypedEventHandler<ReflectionBusObject, Tuple<ReflectionAdapter, IAdapterAttribute>> PropertyChanged;

    }
    public class ReflectionAdapter : AllJoyn.Dsb.AdapterInterface
    {
        object instance;
        Type iface;
        internal ReflectionAdapter(object instance, Type type) : base(GetInterfaceName(type))
        {
            this.instance = instance;
            iface = type;
            bool propertiesEmitsSignalChanged = false;
            foreach (var prop in type.GetTypeInfo().DeclaredProperties)
            {
                var defaultValue = prop.GetCustomAttribute<DefaultValueAttribute>(true).Value;
                var emits = prop.GetCustomAttribute<EmitsSignalChangedAttribute>(true);
                SignalBehavior behavior = SignalBehavior.Unspecified;
                if (emits != null)
                {
                    behavior = emits.Emit ? SignalBehavior.Always : SignalBehavior.Never;
                    propertiesEmitsSignalChanged = propertiesEmitsSignalChanged || emits.Emit;
                }
                // var p = new AllJoyn.Dsb.AdapterAttribute("Version", defaultValue, prop.CanRead, prop.CanWrite) { COVBehavior = behavior };
                // foreach (var anno in prop.GetCustomAttributes<DocStringAttribute>())
                // {
                //     p.Annotations.Add(anno.KeyName, anno.GetValue());
                // }
                // Properties.Add(p);
            }
            if (propertiesEmitsSignalChanged)
            {
                //TODO: Create signal changed events
                if (instance is INotifyPropertyChanged)
                {
                    (instance as INotifyPropertyChanged).PropertyChanged += ReflectionAdapter_PropertyChanged;
                }
            }
        }

        internal event Windows.Foundation.TypedEventHandler<ReflectionAdapter, IAdapterAttribute> PropertyChanged;

        private void ReflectionAdapter_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            var props = Properties.Where(p => p.Value.Name == e.PropertyName).FirstOrDefault();
            if (props != null)
            {
                //TODO: Raise signal changed
                PropertyChanged?.Invoke(this, props);
            }
        }

        private static string GetInterfaceName(Type type)
        {
            var attr = type.GetTypeInfo().GetCustomAttribute<InterfaceNameAttribute>();
            if (attr != null)
                return attr.Name;
            if (type.GetTypeInfo().IsInterface && type.FullName[0] == 'I')
                return type.FullName.Substring(1);
            return type.FullName;
        }
    }
}