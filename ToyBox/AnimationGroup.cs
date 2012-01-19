using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ToyBox
{
    public class AnimationGroup
    {
        private List<Animation> animations;

        public AnimationGroup()
        {
            animations = new List<Animation>();
        }

        public void AttachAnimation(Animation animation)
        {
            animation.Started += new EventHandler<EventArgs>(Animation_Started);
            animation.Finished += new EventHandler<EventArgs>(Animation_Finished);
        }

        private void Animation_Started(object sender, EventArgs args)
        {
            animations.Add((Animation)sender);
        }

        private void Animation_Finished(object sender, EventArgs args)
        {
            animations.Remove((Animation)sender);
        }

        public bool AnimationsActive
        { 
            get
            {
                return animations.Count > 0;
            }
        }
    }
}
