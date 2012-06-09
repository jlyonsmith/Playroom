using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.ObjectModel;

namespace Microsoft.Xna.Framework.Content.Pipeline.Graphics
{
    public sealed class MipmapChainCollection : Collection<MipmapChain>
    {
        private bool fixedSize;

        internal MipmapChainCollection(int initialSize, bool fixedSize)
        {
            for (int i = 0; i < initialSize; i++)
            {
                base.Add(new MipmapChain());
            }
            this.fixedSize = fixedSize;
        }

        protected override void ClearItems()
        {
            if (this.fixedSize)
            {
                throw new NotSupportedException();
            }

            base.ClearItems();
        }

        protected override void InsertItem(int index, MipmapChain item)
        {
            if (this.fixedSize)
            {
                throw new NotSupportedException();
            }
            if (item == null)
            {
                throw new ArgumentNullException("item");
            }
            base.InsertItem(index, item);
        }

        protected override void RemoveItem(int index)
        {
            if (this.fixedSize)
            {
                throw new NotSupportedException();
            }
            base.RemoveItem(index);
        }

        protected override void SetItem(int index, MipmapChain item)
        {
            if (item == null)
            {
                throw new ArgumentNullException("item");
            }
            base.SetItem(index, item);
        }
    }
}
