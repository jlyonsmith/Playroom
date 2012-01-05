using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.IO;

namespace ToyBox
{
    public class SupplyDefaultValueEventArgs : EventArgs
    {
        public string Key { get; private set; }
        public object Value { get; set; }

        public SupplyDefaultValueEventArgs(string key)
        {
            this.Key = key;
            this.Value = null;
        }
    }

    public class PropertyList
    {
        private Dictionary<string, object> dict;
        
        public event EventHandler<SupplyDefaultValueEventArgs> SupplyDefaultValue;

        public PropertyList()
        {
            this.dict = new Dictionary<string, object>();
        }

        public PropertyList(Dictionary<string, object> dict)
        {
            this.dict = dict;
        }

        public object this[string name]
        {
            get
            {
                object obj;

                if (dict.TryGetValue(name, out obj))
                    return obj;

                obj = RaiseSupplyDefaultValueEvent(name);
                dict[name] = obj;
                return obj;
            }
            set
            {
                dict[name] = value;
            }
        }

        public string ToXml()
        {
            StringBuilder sb = new StringBuilder();

            using (XmlWriter xw = XmlWriter.Create(sb))
            {
                PropertyListWriter.WriteXml(xw, dict);
            }

            return sb.ToString();
        }

        public static PropertyList FromXml(string xml)
        {
            PropertyList propertyList = null;

            try
            {
                using (StringReader sr = new StringReader(xml))
                {
                    using (XmlReader xr = XmlReader.Create(sr))
                    {
                        propertyList = PropertyListReaderV1.ReadXml(xr);
                    }
                }
            }
            catch (Exception)
            {
            }

            return propertyList;
        }

        private object RaiseSupplyDefaultValueEvent(string name)
        {
            SupplyDefaultValueEventArgs args = new SupplyDefaultValueEventArgs(name);

            if (SupplyDefaultValue != null)
            {
                foreach (var method in SupplyDefaultValue.GetInvocationList())
                {
                    method.Method.Invoke(SupplyDefaultValue.Target, new object[] {this, args});

                    if (args.Value != null)
                        break;
                }
            }

            return args.Value;
        }
    }
}
