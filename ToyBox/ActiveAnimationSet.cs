using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ToyBox
{
    public class ActiveAnimationSet : Set<Animation>
    {
        public ActiveAnimationSet()
        {
        }

        public override void Add(Animation animation)
        {
            animation.Started += new EventHandler<EventArgs>(Animation_Started);
            animation.Finished += new EventHandler<EventArgs>(Animation_Finished);
        }

        private void Animation_Started(object sender, EventArgs args)
        {
            base.Add((Animation)sender);
        }

        private void Animation_Finished(object sender, EventArgs args)
        {
            base.Remove((Animation)sender);
        }
    }
}
