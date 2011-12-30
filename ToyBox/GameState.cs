using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace ToyBox
{
    public abstract class GameState : IGameState, IUpdateable
    {
        protected Game Game { get; set; }
        
        public GameState(Game game)
        {
            this.Game = game;
        }

        event EventHandler<System.EventArgs> IUpdateable.EnabledChanged { add { } remove { } }
        event EventHandler<System.EventArgs> IUpdateable.UpdateOrderChanged { add { } remove { } }

        public void Pause()
        {
            if (!this.paused)
            {
                OnPause();
                this.paused = true;
            }
        }

        public void Resume()
        {
            if (this.paused)
            {
                OnResume();
                this.paused = false;
            }
        }

        public abstract void Update(GameTime gameTime);

        protected virtual void OnEntered() { }
        protected virtual void OnLeaving() { }
        protected virtual void OnPause() { }
        protected virtual void OnResume() { }

        protected bool Paused
        {
            get { return this.paused; }
        }

        void IGameState.Enter()
        {
            OnEntered();
        }

        void IGameState.Leave()
        {
            OnLeaving();
        }

        bool IUpdateable.Enabled
        {
            get { return true; }
        }

        int IUpdateable.UpdateOrder
        {
            get { return 0; }
        }

        private bool paused;
    }
}
