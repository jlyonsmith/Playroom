using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace ToyBox
{
    public class GameScreenManager : DrawableGameComponent, IGameScreenService, IDisposable
    {
        private bool disposeDroppedStates;
        private List<KeyValuePair<IGameScreen, GameScreenModality>> gameStates;
        private List<IUpdateable> updateableStates;
        private List<IDrawable> drawableStates;

        public GameScreenManager(Game game) :
            base(game)
        {
            this.gameStates = new List<KeyValuePair<IGameScreen, GameScreenModality>>();
            this.updateableStates = new List<IUpdateable>();
            this.drawableStates = new List<IDrawable>();

            if (game.Services != null)
                game.Services.AddService(typeof(IGameScreenService), this);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                LeaveAllActiveStates();

                if (this.Game.Services != null)
                {
                    this.Game.Services.RemoveService(typeof(IGameScreenService));
                }
            }

            base.Dispose(disposing);
        }

        public bool DisposeDroppedStates
        {
            get { return this.disposeDroppedStates; }
            set { this.disposeDroppedStates = value; }
        }

        public void Pause()
        {
            if (this.gameStates.Count > 0)
            {
                this.gameStates[this.gameStates.Count - 1].Key.Pause();
            }
        }

        public void Resume()
        {
            if (this.gameStates.Count > 0)
            {
                this.gameStates[this.gameStates.Count - 1].Key.Resume();
            }
        }

        public void Push(IGameScreen screen)
        {
            Push(screen, GameScreenModality.Exclusive);
        }

        public void Push(IGameScreen screen, GameScreenModality modality)
        {
            Pause();

            // If this game screen is modal, take all game screens that came before it
            // from the draw and update lists
            if (modality == GameScreenModality.Exclusive)
            {
                this.drawableStates.Clear();
                this.updateableStates.Clear();
            }

            // Add the new screen to the update and draw lists if it implements
            // the required interfaces
            this.gameStates.Add(new KeyValuePair<IGameScreen, GameScreenModality>(screen, modality));
            AppendToUpdateableAndDrawableList(screen);

            // State is set, now try to enter it
#if DEBUG
            screen.Enter();
#else
            try
            {
                screen.Enter();
            }
            catch (Exception)
            {
                Pop();
                throw;
            }
#endif
        }

        public IGameScreen Pop()
        {
            int lastStateIndex = this.gameStates.Count - 1;
            
            if (lastStateIndex < 0)
            {
                throw new InvalidOperationException("No game screens are on the stack");
            }

            KeyValuePair<IGameScreen, GameScreenModality> old = this.gameStates[lastStateIndex];

            // Notify the currently active screen that it's being left and take it
            // from the stack of active screens
            old.Key.Leave();
            this.gameStates.RemoveAt(lastStateIndex);

            // Now we need to remove the popped screen from our update and draw lists.
            // If the popped screen was exclusive, our lists are empty and we need to
            // rebuild them. Otherwise, we can simply remove the lastmost entry.
            if (old.Value == GameScreenModality.Exclusive)
            {
                this.updateableStates.Clear();
                this.drawableStates.Clear();
                RebuildUpdateableAndDrawableListRecursively(lastStateIndex - 1);
            }
            else
            {
                RemoveFromUpdateableAndDrawableList(old.Key);
            }

            // If the user desires so, dispose the dropped screen
            DisposeIfSupportedAndDesired(old.Key);

            // Resume the screen that has now become the top of the stack
            Resume();

            return old.Key;
        }

        public IGameScreen Switch(IGameScreen screen)
        {
            return Switch(screen, GameScreenModality.Exclusive);
        }

        public IGameScreen Switch(IGameScreen screen, GameScreenModality modality)
        {
            int screenCount = this.gameStates.Count;
            
            if (screenCount == 0)
            {
                Push(screen, modality);
                return null;
            }

            int lastStateIndex = screenCount - 1;
            KeyValuePair<IGameScreen, GameScreenModality> old = this.gameStates[lastStateIndex];
            IGameScreen previousState = old.Key;

            // Notify the previous screen that it's being left and kill it if desired
            previousState.Leave();
            DisposeIfSupportedAndDesired(previousState);

            // If the switched-to screen is exclusive, we need to clear the update
            // and draw lists. If not, depending on whether the previous screen was
            // a popup screen, we might have to 
            if (old.Value == GameScreenModality.Popup)
            {
                RemoveFromUpdateableAndDrawableList(previousState);
            }
            else
            {
                this.updateableStates.Clear();
                this.drawableStates.Clear();
            }

            // Now swap out the screen and put it in the update and draw lists. If we're
            // switching from an exclusive to a pop-up screen, the draw and update lists need
            // to be rebuilt.
            var newState = new KeyValuePair<IGameScreen, GameScreenModality>(screen, modality);
            this.gameStates[lastStateIndex] = newState;
            if (old.Value == GameScreenModality.Exclusive && modality == GameScreenModality.Popup)
            {
                RebuildUpdateableAndDrawableListRecursively(lastStateIndex);
            }
            else
            {
                AppendToUpdateableAndDrawableList(screen);
            }

            // Let the screen know that it has been entered
            screen.Enter();

            return previousState;
        }

        public IGameScreen ActiveState
        {
            get
            {
                int count = this.gameStates.Count;
                if (count == 0)
                {
                    return null;
                }
                else
                {
                    return this.gameStates[count - 1].Key;
                }
            }
        }

        public override void Update(GameTime gameTime)
        {
            for (int index = 0; index < this.updateableStates.Count; ++index)
            {
                var updateable = this.updateableStates[index];
                if (updateable.Enabled)
                {
                    updateable.Update(gameTime);
                }
            }
        }

        public override void Draw(GameTime gameTime)
        {
            for (int index = 0; index < this.drawableStates.Count; ++index)
            {
                var drawable = this.drawableStates[index];
                if (drawable.Visible)
                {
                    this.drawableStates[index].Draw(gameTime);
                }
            }
        }

        private void DisposeIfSupportedAndDesired(IGameScreen screen)
        {
            if (this.disposeDroppedStates)
            {
                var disposable = screen as IDisposable;
                if (disposable != null)
                {
                    disposable.Dispose();
                }
            }
        }

        private void RebuildUpdateableAndDrawableListRecursively(int index)
        {
            if (index < 0)
            {
                return;
            }

            if (this.gameStates[index].Value != GameScreenModality.Exclusive)
            {
                RebuildUpdateableAndDrawableListRecursively(index - 1);
            }

            AppendToUpdateableAndDrawableList(this.gameStates[index].Key);
        }

        private void RemoveFromUpdateableAndDrawableList(IGameScreen screen)
        {
            int lastDrawableIndex = this.drawableStates.Count - 1;
            
            if (lastDrawableIndex > -1)
            {
                if (ReferenceEquals(this.drawableStates[lastDrawableIndex], screen))
                {
                    this.drawableStates.RemoveAt(lastDrawableIndex);
                }
            }

            int lastUpdateableIndex = this.updateableStates.Count - 1;
            if (lastUpdateableIndex > -1)
            {
                if (ReferenceEquals(this.updateableStates[lastUpdateableIndex], screen))
                {
                    this.updateableStates.RemoveAt(lastUpdateableIndex);
                }
            }
        }

        private void LeaveAllActiveStates()
        {
            for (int index = this.gameStates.Count - 1; index >= 0; --index)
            {
                IGameScreen screen = this.gameStates[index].Key;
                screen.Leave();
                DisposeIfSupportedAndDesired(screen);
                this.gameStates.RemoveAt(index);
            }

            this.drawableStates.Clear();
            this.updateableStates.Clear();
        }

        private void AppendToUpdateableAndDrawableList(IGameScreen screen)
        {
            IUpdateable updateable = screen as IUpdateable;
            if (updateable != null)
            {
                this.updateableStates.Add(updateable);
            }

            IDrawable drawable = screen as IDrawable;
            if (drawable != null)
            {
                this.drawableStates.Add(drawable);
            }
        }
    }
} 
