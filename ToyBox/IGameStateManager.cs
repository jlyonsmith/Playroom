﻿using System;
using System.Collections.Generic;
using System.Text;

namespace ToyBox
{
    public interface IGameStateService
    {
        /// <summary>The currently active game state. Can be null.</summary>
        IGameState ActiveState { get; }

        /// <summary>Pauses the currently active state</summary>
        void Pause();

        /// <summary>Resumes the currently active state</summary>
        void Resume();

        /// <summary>Pushes the specified state onto the state stack</summary>
        /// <param name="state">State that will be pushed onto the stack</param>
        void Push(IGameState state);

        /// <summary>Pushes the specified state onto the state stack</summary>
        /// <param name="state">State that will be pushed onto the stack</param>
        /// <param name="modality">
        ///   Behavior of the game state in relation to the state(s) below it on the stack
        /// </param>
        void Push(IGameState state, GameStateModality modality);

        /// <summary>Takes the currently active game state from the stack</summary>
        /// <returns>The game state that has been popped from the stack</returns>
        IGameState Pop();

        /// <summary>Switches the game to the specified state</summary>
        /// <param name="state">State the game will be switched to</param>
        /// <returns>The game state that was replaced on the stack</returns>
        /// <remarks>
        ///   This replaces the running game state in the stack with the specified state.
        /// </remarks>
        IGameState Switch(IGameState state);

        /// <summary>Switches the game to the specified state</summary>
        /// <param name="state">State the game will be switched to</param>
        /// <param name="modality">
        ///   Behavior of the game state in relation to the state(s) below it on the stack
        /// </param>
        /// <returns>The game state that was replaced on the stack</returns>
        /// <remarks>
        ///   This replaces the running game state in the stack with the specified state.
        /// </remarks>
        IGameState Switch(IGameState state, GameStateModality modality);
    }

    public enum GameStateModality
    {
        /// <summary>
        /// The game state takes exclusive of the screen does not require the state
        /// below it in the stack to be updated as long as it's active.
        /// </summary>
        Exclusive,

        /// <summary>
        /// The game state sits on top of the state below it in the stack, but does
        /// not completely obscure it or requires it to continue being updated.
        /// </summary>
        Popup
    }
}
