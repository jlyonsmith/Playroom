using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace ToyBox
{
    public interface IStorageService
    {
        string Load(string contentName);
        void Save(string contentName, string content);
    }
}
