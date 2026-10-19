using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Windows.Forms;
using ClipCourier.Platform;

namespace ClipCourier.Models
{
    [DataContract]
    internal sealed class AppConfig
    {
        [DataMember(Order = 1)] public int HotkeyKey;
        [DataMember(Order = 2)] public uint HotkeyModifiers;
        [DataMember(Order = 3)] public bool LaunchAtLogin;
        [DataMember(Order = 4)] public List<Destination> Destinations;

        public AppConfig()
        {
            HotkeyKey = (int)Keys.J;
            HotkeyModifiers = Native.ModControl | Native.ModAlt;
            LaunchAtLogin = false;
            Destinations = new List<Destination>();
        }

        public AppConfig Copy()
        {
            AppConfig copy = new AppConfig();
            copy.HotkeyKey = HotkeyKey;
            copy.HotkeyModifiers = HotkeyModifiers;
            copy.LaunchAtLogin = LaunchAtLogin;
            copy.Destinations = new List<Destination>();
            if (Destinations != null)
                foreach (Destination d in Destinations) copy.Destinations.Add(new Destination(d.Name, d.Path));
            return copy;
        }
    }

}
