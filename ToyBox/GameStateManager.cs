using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace ToyBox
{
    public class GameStateManager : DrawableGameComponent, IGameStateService, IDisposable
    {
        private bool disposeDroppedStates;
        private List<KeyValuePair<IGameState, GameStateModality>> gameStates;
        private List<IUpdateable> updateableStates;
        private List<IDrawable> drawableStates;

        public GameStateManager(Game game) :
            base(game)
        {
            this.gameStates = new List<KeyValuePair<IGameState, GameStateModality>>();
            this.updateableStates = new List<IUpdateable>();
            this.drawableStates = new List<IDrawable>();

            if (game.Services != null)
                game.Services.AddService(typeof(IGameStateService), this);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                LeaveAllActiveStates();

                if (this.Game.Services != null)
                {
                    this.Game.Services.RemoveService(typeof(IGameStateService));
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

        public void Push(IGameState state)
        {
            Push(state, GameStateModality.Exclusive);
        }

        public void Push(IGameState state, GameStateModality modality)
        {
            Pause();

            // If this game state is modal, take all game states that came before it
            // from the draw and update lists
            if (modality == GameStateModality.Exclusive)
            {
                this.drawableStates.Clear();
                this.updateableStates.Clear();
            }

            // Add the new state to the update and draw lists if it implements
            // the required interfaces
            this.gameStates.Add(new KeyValuePair<IGameState, GameStateModality>(state, modality));
            AppendToUpdateableAndDrawableList(state);

            // State is set, now try to enter it
#if DEBUG
            state.Enter();
#else
            try
            {
                state.Enter();
            }
            catch (Exception)
            {
                Pop();
                throw;
            }
#endif
        }

        public IGameState Pop()
        {
            int lastStateIndex = this.gameStates.Count - 1;
            
            if (lastStateIndex < 0)
            {
                throw new InvalidOperationException("No game states are on the stack");
            }

            KeyValuePair<IGameState, GameStateModality> old = this.gameStates[lastStateIndex];

            // Notify the currently active state that it's being left and take it
            // from the stack of active states
            old.Key.Leave();
            this.gameStates.RemoveAt(lastStateIndex);

            // Now we need to remove the popped state from our update and draw lists.
            // If the popped state was exclusive, our lists are empty and we need to
            // rebuild them. Otherwise, we can simply remove the lastmost entry.
            if (old.Value == GameStateModality.Exclusive)
            {
                this.updateableStates.Clear();
                this.drawableStates.Clear();
                RebuildUpdateableAndDrawableListRecursively(lastStateIndex - 1);
            }
            else
            {
                RemoveFromUpdateableAndDrawableList(old.Key);
            }

            // If the user desires so, dispose the dropped state
            DisposeIfSupportedAndDesired(old.Key);

            // Resume the state that has now become the top of the stack
            Resume();

            return old.Key;
        }

        public IGameState Switch(IGameState state)
        {
            return Switch(state, GameStateModality.Exclusive);
        }

        public IGameState Switch(IGameState state, GameStateModality modality)
        {
            int stateCount = this.gameStates.Count;
            
            if (stateCount == 0)
            {
                Push(state, modality);
                return null;
            }

            int lastStateIndex = stateCount - 1;
            KeyValuePair<IGameState, GameStateModality> old = this.gameStates[lastStateIndex];
            IGameState previousState = old.Key;

            // Notify the previous state that it's being left and kill it if desired
            previousState.Leave();
            DisposeIfSupportedAndDesired(previousState);

            // If the switched-to state is exclusive, we need to clear the update
            // and draw lists. If not, depending on whether the previous state was
            // a popup state, we might have to 
            if (old.Value == GameStateModality.Popup)
            {
                RemoveFromUpdateableAndDrawableList(previousState);
            }
            else
            {
                this.updateableStates.Clear();
                this.drawableStates.Clear();
            }

            // Now swap out the state and put it in the update and draw lists. If we're
            // switching from an exclusive to a pop-up state, the draw and update lists need
            // to be rebuilt.
            var newState = new KeyValuePair<IGameState, GameStateModality>(state, modality);
            this.gameStates[lastStateIndex] = newState;
            if (old.Value == GameStateModality.Exclusive && modality == GameStateModality.Popup)
            {
                RebuildUpdateableAndDrawableListRecursively(lastStateIndex);
            }
            else
            {
                AppendToUpdateableAndDrawableList(state);
            }

            // Let the state know that it has been entered
            state.Enter();

            return previousState;
        }

        public IGameState ActiveState
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

        private void DisposeIfSupportedAndDesired(IGameState state)
        {
            if (this.disposeDroppedStates)
            {
                var disposable = state as IDisposable;
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

            if (this.gameStates[index].Value != GameStateModality.Exclusive)
            {
                RebuildUpdateableAndDrawableListRecursively(index - 1);
            }

            AppendToUpdateableAndDrawableList(this.gameStates[index].Key);
        }

        private void RemoveFromUpdateableAndDrawableList(IGameState state)
        {
            int lastDrawableIndex = this.drawableStates.Count - 1;
            
            if (lastDrawableIndex > -1)
            {
                if (ReferenceEquals(this.drawableStates[lastDrawableIndex], state))
                {
                    this.drawableStates.RemoveAt(lastDrawableIndex);
                }
            }

            int lastUpdateableIndex = this.updateableStates.Count - 1;
            if (lastUpdateableIndex > -1)
            {
                if (ReferenceEquals(this.updateableStates[lastUpdateableIndex], state))
                {
                    this.updateableStates.RemoveAt(lastUpdateableIndex);
                }
            }
        }

        private void LeaveAllActiveStates()
        {
            for (int index = this.gameStates.Count - 1; index >= 0; --index)
            {
                IGameState state = this.gameStates[index].Key;
                state.Leave();
                DisposeIfSupportedAndDesired(state);
                this.gameStates.RemoveAt(index);
            }

            this.drawableStates.Clear();
            this.updateableStates.Clear();
        }

        private void AppendToUpdateableAndDrawableList(IGameState state)
        {
            IUpdateable updateable = state as IUpdateable;
            if (updateable != null)
            {
                this.updateableStates.Add(updateable);
            }

            IDrawable drawable = state as IDrawable;
            if (drawable != null)
            {
                this.drawableStates.Add(drawable);
            }
        }
    }
} 
