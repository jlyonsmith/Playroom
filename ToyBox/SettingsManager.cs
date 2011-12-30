using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using System.Xml;
using System.IO;

namespace ToyBox
{
    public class SettingsManager : GameComponent, ISettingsService
    {
        private Dictionary<string, object> settings;
        private EventHandler<SupplyDefaultValueEventArgs> supplyDefaultValue;

        public SettingsManager(Game game) : base(game)
        {
            settings = new Dictionary<string, object>();

            if (game.Services != null)
                game.Services.AddService(typeof(ISettingsService), this);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (this.Game.Services != null)
                {
                    this.Game.Services.RemoveService(typeof(ISettingsService));
                }
            }

            base.Dispose(disposing);
        }

        #region ISettingsService Members

        public event EventHandler<SupplyDefaultValueEventArgs> SupplyDefaultValue
        {
            add { supplyDefaultValue += value; }
            remove { supplyDefaultValue -= value; }
        }

        public object this[string name]
        {
            get
            {
                object obj;

                if (settings.TryGetValue(name, out obj))
                    return obj;

                obj = RaiseSupplyDefaultValueEvent(name);
                settings[name] = obj;
                return obj;
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        public void Load()
        {
            /*
        try
        {
            ParsedPath path = this.OptionsFileName;

            if (!Directory.Exists(path.VolumeAndDirectory))
            {
                Directory.CreateDirectory(path.VolumeAndDirectoryNoSeparator);
            }

            if (!File.Exists(path))
            {
                options = Options.Default;
                SaveOptions();
                return;
            }

            using (StreamReader sr = new StreamReader(path))
            {
                using (XmlReader xr = XmlReader.Create(sr))
                {
                    this.options = OptionsReaderV1.ReadXml(xr);
                }
            }
        }
        catch (Exception e)
        {
            //options = Options.Default;
        }
            */
        }

        public void Save()
        {
            using (StreamWriter sw = new StreamWriter(""))
            {
                using (XmlWriter xw = XmlWriter.Create(sw))
                {
                    SettingsWriter.WriteXml(xw, settings);
                }
            }
        }

        public object GetDefault(string name)
        {
            return RaiseSupplyDefaultValueEvent(name);
        }

        #endregion

        private object RaiseSupplyDefaultValueEvent(string name)
        {
            SupplyDefaultValueEventArgs args = new SupplyDefaultValueEventArgs(name);

            if (supplyDefaultValue != null)
            {
                foreach (var method in supplyDefaultValue.GetInvocationList())
                {
                    method.Method.Invoke(supplyDefaultValue.Target, new object[] {this, args});

                    if (args.Value != null)
                        break;
                }
            }

            return args.Value;
        }
    }
}
