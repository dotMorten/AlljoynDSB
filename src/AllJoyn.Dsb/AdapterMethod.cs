using BridgeRT;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AllJoyn.Dsb
{
    //
    // AdapterMethod.
    // Description:
    // The class that implements IAdapterMethod from BridgeRT.
    //
    public sealed class AdapterMethod : IAdapterMethod
    {
        // public properties
        public string Name { get; }

        public string Description { get; }

        IList<IAdapterValue> IAdapterMethod.InputParams { get; set; } = new List<IAdapterValue>();

        IList<IAdapterValue> IAdapterMethod.OutputParams { get; } = new List<IAdapterValue>();

        int m_hresult = 0;
        int IAdapterMethod.HResult { get { return m_hresult; } }

        public AdapterMethod(
            string ObjectName,
            string Description, InvokeMethodDelegate invokeAction, IEnumerable<IAdapterValue> inputs = null, IEnumerable<IAdapterValue> outputs = null)
        {
            this.Name = ObjectName;
            this.Description = Description;
            if (invokeAction == null)
                throw new ArgumentNullException(nameof(invokeAction));
            this.InvokeAction = invokeAction;
            
            if(inputs != null && inputs.Any())
            {
                foreach(var item in inputs)
                {
                    ((IAdapterMethod)this).InputParams.Add(item);
                }
            }
            if (outputs != null && outputs.Any())
            {
                var d = new Dictionary<string, object>();
                foreach (var item in outputs)
                {
                    ((IAdapterMethod)this).OutputParams.Add(item);
                }
            }
        }
        
        public delegate void InvokeMethodDelegate(AdapterMethod sender, IReadOnlyDictionary<string, object> inputParams, IDictionary<string, object> outputParams);
        
        internal InvokeMethodDelegate InvokeAction { get; }

        internal void Invoke()
        {
            int result = 0;
            if (InvokeAction != null)
            {
                try
                {
                    //Create input and output collectionsNo sure 
                    System.Collections.ObjectModel.ReadOnlyDictionary<string, object> input = null;
                    var inParms = ((IAdapterMethod)this).InputParams;
                    if (inParms != null && inParms.Any())
                    {
                        var d = new Dictionary<string, object>();
                        foreach (var item in inParms)
                            d[item.Name] = item.Data;
                        input = new System.Collections.ObjectModel.ReadOnlyDictionary<string, object>(d);
                    }
                    Dictionary<string, object> output = null;
                    var outParms = ((IAdapterMethod)this).OutputParams;
                    if (outParms != null && outParms.Any())
                    {
                        output = new Dictionary<string, object>();
                        foreach (var item in outParms)
                            output[item.Name] = item.Data;
                    }
                    InvokeAction(this, input, output);
                    if(output != null)
                    {
                        foreach(var item in outParms)
                        {
                            if (output.ContainsKey(item.Name))
                                item.Data = output[item.Name];
                        }
                    }
                }
                catch(System.Exception ex)
                {
                    if (System.Diagnostics.Debugger.IsAttached)
                        throw ex;
                    result = ex.HResult;
                    if (result == 0) result = 1;
                }
            }
            m_hresult = result;
        }
    }
}
