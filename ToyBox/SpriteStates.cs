using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ToyBox
{
    public class SpriteState
    {
        public bool? Visible { get; set; }
    }

    public class SpriteStates
    {
        private Dictionary<Sprite, SpriteState> dict = new Dictionary<Sprite, SpriteState>();

        public SpriteStates()
        {
        }

        public void SaveVisibility(Sprite sprite)
        {
            GetSpriteState(sprite).Visible = sprite.Visible;    
        }

        private SpriteState GetSpriteState(Sprite sprite)
        {
            SpriteState props;

            if (!dict.TryGetValue(sprite, out props))
            {
                props = new SpriteState();
                dict.Add(sprite, props);
            }

            return props;
        }

        public void Clear()
        {
            dict.Clear();
        }

        public void Restore()
        {
            foreach (var pair in dict)
            {
                if (pair.Value.Visible.HasValue)
                    pair.Key.Visible = pair.Value.Visible.Value;
            }
        }
    }
}
