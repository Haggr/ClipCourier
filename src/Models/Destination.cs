using System.Runtime.Serialization;

namespace ClipCourier.Models
{
    [DataContract]
    internal sealed class Destination
    {
        [DataMember(Order = 1)] public string Name;
        [DataMember(Order = 2)] public string Path;

        public Destination() { Name = ""; Path = ""; }
        public Destination(string name, string path) { Name = name; Path = path; }
        public override string ToString() { return Name; }
    }

}
