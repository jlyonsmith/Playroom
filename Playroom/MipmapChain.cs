using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;

namespace Playroom
{
    public sealed class MipmapChain : Collection<BitmapContent>
    {
        public MipmapChain()
        {
        }

        public MipmapChain(BitmapContent bitmap)
        {
            base.Add(bitmap);
        }

        protected override void InsertItem(int index, BitmapContent item)
        {
            if (item == null)
            {
                throw new ArgumentNullException("item");
            }
            base.InsertItem(index, item);
        }

        public static implicit operator MipmapChain(BitmapContent bitmap)
        {
            return new MipmapChain(bitmap);
        }

        protected override void SetItem(int index, BitmapContent item)
        {
            if (item == null)
            {
                throw new ArgumentNullException("item");
            }
            base.SetItem(index, item);
        }
    }
}
