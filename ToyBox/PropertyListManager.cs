using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace ToyBox
{
    public class PropertyListManager : GameComponent, IPropertyListService
    {
        private Dictionary<string, PropertyList> lists = new Dictionary<string, PropertyList>();
        private IStorageService storageService;

        public PropertyListManager(Game game) : base(game)
        {
            if (game.Services != null)
                game.Services.AddService(typeof(IPropertyListService), this);

            storageService = (IStorageService)game.Services.GetService(typeof(IStorageService));
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (this.Game.Services != null)
                {
                    this.Game.Services.RemoveService(typeof(IPropertyListService));
                }
            }

            base.Dispose(disposing);
        }

        #region IPropertyListService Members

        public void Load(string propertyListName, EventHandler<SupplyDefaultValueEventArgs> handler)
        {
            string xml = storageService.LoadString(propertyListName);
            PropertyList propList;

            if (!this.lists.TryGetValue(propertyListName, out propList))
            {
                propList = new PropertyList();
                storageService.SaveString(propertyListName, propList.ToString());
            }

            if (xml != null)
            {
                propList = PropertyList.FromXml(xml);
            }

            if (propList == null)
            {
                propList = new PropertyList();
                storageService.SaveString(propertyListName, propList.ToString());
            }

            propList.SupplyDefaultValue += new EventHandler<SupplyDefaultValueEventArgs>(handler);
            
            lists.Add(propertyListName, propList);
        }

        public PropertyList Get(string propertyListName)
        {
            return lists[propertyListName];
        }

        public void Save(string propertyListName)
        {
            this.storageService.SaveString(propertyListName, lists[propertyListName].ToString());
        }

        #endregion
    }
}
