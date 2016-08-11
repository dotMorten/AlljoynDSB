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
    public class ReflectionAdapter<T> : AllJoyn.Dsb.AdapterInterface
    {
        public ReflectionAdapter(T iface) : base(GetInterfaceName())
        {
            var type = typeof(T);
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
                var p = new AllJoyn.Dsb.AdapterAttribute("Version", defaultValue, prop.CanRead, prop.CanWrite) { COVBehavior = behavior };
                foreach (var anno in prop.GetCustomAttributes<DocStringAttribute>())
                {
                    p.Annotations.Add(anno.KeyName, anno.GetValue());
                }
                Properties.Add(p);
            }
            if (propertiesEmitsSignalChanged)
            {
                //TODO: Create signal changed events
                if (iface is INotifyPropertyChanged)
                {
                    (iface as INotifyPropertyChanged).PropertyChanged += ReflectionAdapter_PropertyChanged;
                }
            }
        }

        private void ReflectionAdapter_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            var props = Properties.Where(p => p.Value.Name == e.PropertyName).FirstOrDefault();
            if (props != null)
            {
                //TODO: Raise signal changed
            }
        }

        private static string GetInterfaceName()
        {
            var type = typeof(T);
            var attr = type.GetTypeInfo().GetCustomAttribute<InterfaceNameAttribute>();
            if (attr != null)
                return attr.Name;
            if (type.GetTypeInfo().IsInterface && type.FullName[0] == 'I')
                return type.FullName.Substring(1);
            return type.FullName;
        }
    }
}