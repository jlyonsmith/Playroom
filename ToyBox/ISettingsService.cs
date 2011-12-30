using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace ToyBox
{
    public class SupplyDefaultValueEventArgs : EventArgs
    {
        public string Name { get; private set; }
        public object Value { get; set; }

        public SupplyDefaultValueEventArgs(string name)
        {
            this.Name = name;
        }
    }

    public interface ISettingsService
    {
        event EventHandler<SupplyDefaultValueEventArgs> SupplyDefaultValue;
        object this[string name] { get; set; }
        void Load();
        void Save();
    }
}
