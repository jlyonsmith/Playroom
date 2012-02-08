using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace ToyBox
{
    public interface IStorageService
    {
        string LoadString(string contentName);
        PropertyList LoadOrCreatePropertyList(string propertyListName, EventHandler<SupplyDefaultValueEventArgs> handler);
        void SaveString(string contentName, string content);
        void SavePropertyList(string propertyListName, PropertyList propertyList);
    }
}
