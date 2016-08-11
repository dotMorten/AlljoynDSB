using BridgeRT;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AllJoyn.Dsb
{
    //
    // AdapterAttribute.
    // Description:
    // The class that implements IAdapterAttribute from BridgeRT.
    //
    public sealed class AdapterAttribute : IAdapterAttribute
    {
        // public properties
        public IAdapterValue Value { get; }

        public E_ACCESS_TYPE Access { get; set; }
        public IDictionary<string, string> Annotations { get; }
        public SignalBehavior COVBehavior { get; set; }

        public AdapterAttribute(string ObjectName, object DefaultData, bool canWrite = false, bool canRead = true)
        {
            try
            {
                this.Value = new AdapterValue(ObjectName, DefaultData);
                this.Annotations = new Dictionary<string, string>();
                if (!canRead && !canWrite)
                    throw new ArgumentException("A property must be able to either read, write or both", nameof(canRead));
                if (canRead && canWrite)
                    Access = E_ACCESS_TYPE.ACCESS_READWRITE;
                else if (canRead)
                    Access = E_ACCESS_TYPE.ACCESS_READ;
                else if (canWrite)
                    Access = E_ACCESS_TYPE.ACCESS_WRITE;
                this.COVBehavior = SignalBehavior.Never;
            }
            catch (OutOfMemoryException)
            {
                throw;
            }
        }

        internal AdapterAttribute(AdapterAttribute Other)
        {
            this.Value = Other.Value;
            this.Annotations = Other.Annotations;
            this.Access = Other.Access;
            this.COVBehavior = Other.COVBehavior;
        }
    }
}
