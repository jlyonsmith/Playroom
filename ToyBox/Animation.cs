using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using System.Collections;
using System.Collections.ObjectModel;

namespace ToyBox
{
    public abstract class Animation
    {
        private TimeSpan time = TimeSpan.Zero;

        protected SpriteManager SpriteManager { get; set; }

        public bool HasStarted { get; private set; }
        public bool HasFinished { get; private set; }
        public Sprite Sprite { get; private set; }
        public TimeSpan StartDelay { get; private set; }
        public TimeSpan Duration { get; private set; }
        public Animation NextAnimation { get; set; }

        public event EventHandler<EventArgs> Finished;
        public event EventHandler<EventArgs> Started;

        public Animation(TimeSpan startDelay, TimeSpan duration)
        {
            this.StartDelay = startDelay;
            this.Duration = duration;
        }

        public virtual void OnInitialize(SpriteManager spriteManager, Sprite sprite)
        {
            this.SpriteManager = spriteManager;
            this.Sprite = sprite;
            this.HasStarted = false;
            this.HasFinished = false;
        }

        public void Update(GameTime gameTime)
        {
            time += gameTime.ElapsedGameTime;

            if (time < StartDelay)
            {
                return;
            }
            else if (time >= StartDelay + Duration)
            {
                OnFinish();

                this.Sprite.ActiveAnimation = null;
                this.HasFinished = true;

                EventHandler<EventArgs> handler = this.Finished;

                if (handler != null)
                    handler(this, new EventArgs());

                if (this.NextAnimation != null)
                {
                    this.SpriteManager.ActivateNextAnimation(this.Sprite, this.NextAnimation);
                }
            }
            else
            {
                if (!HasStarted)
                {
                    OnStart();
                    this.HasStarted = true;

                    EventHandler<EventArgs> handler = this.Started;

                    if (handler != null)
                        handler(this, new EventArgs());
                }

                OnAnimate((float)(time.Ticks - StartDelay.Ticks) / (float)Duration.Ticks);
            }
        }

        public abstract void OnStart();
        public abstract void OnAnimate(float t);
        public abstract void OnFinish();
    }
}