using System;
using System.Collections;
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
            // TODO-john-2012: This should do a deep copy of the passed in dictionary
            this.dict = dict;
        }

        public PropertyList DeepClone()
        {
            PropertyList propList = new PropertyList();

            propList.dict = CloneDictionary(this.dict);
            propList.SupplyDefaultValue = this.SupplyDefaultValue;

            return propList;
        }

        private Dictionary<string, object> CloneDictionary(Dictionary<string, object> fromDict)
        {
            Dictionary<string, object> newDict = new Dictionary<string,object>();

            foreach (var pair in fromDict)
            {
                if (pair.Value is Dictionary<string, object>)
                {
                    newDict.Add(pair.Key, CloneDictionary((Dictionary<string, object>)pair.Value));
                }
                else if (pair.Value is List<object>)
                {
                    throw new NotImplementedException();
                }
                else
                {
                    newDict.Add(pair.Key, pair.Value);
                }
            }

            return newDict;
        }

        // TODO-john-2012: Replace with Get/Set methods for the valid list of data types
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

        // TODO-john-2012: Override ToString with a parameter instead
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
