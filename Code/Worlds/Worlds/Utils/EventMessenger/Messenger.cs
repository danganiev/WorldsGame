// Messenger.cs v0.1 (20090925) by Rod Hyde (badlydrawnrod).

//
// This is a C# messenger (notification center) for Unity. It uses delegates
// and generics to provide type-checked messaging between event producers and
// event consumers, without the need for producers or consumers to be aware of
// each other.

#region Events list

// Events list:
//
// -------------------(Client & Singleplayer)-----------------------
// (Keyboard & mouse)
// *All keyboard events are in the InputController.cs*
//
// MouseLeftButtonClick
// MouseRightButtonClick
// MouseLeftButtonPressed
// MouseRightButtonPressed
// MouseMiddleButtonPressed
// MouseLeftButtonReleased
// MouseRightButtonReleased
// MouseMiddleButtonReleased
//
// MouseDeltaXChange<float>
// MouseDeltaYChange<float>
// MouseDeltaChange
//
// ScrollWheelDeltaChanged<int>
//
// ToggleMouseCentering
//
// (GUI)
//
// PauseMenuStart
// PauseMenuStop
//
// (World)
// WorldBlockChange
//
// (Chunk)
// ChunkDisposing
//
// (Player)
// SelectedBlockChange
// PlayerMovementChanged
// PlayerJumped
// PlayerInventoryToggle
// PlayerInventoryUpdate
// PlayerDied

//
// (LoadingState)
// LoadingMessageChange
//
// (Multiplayer)
//
// (Object creation only)
//
// -------------------(Server)-----------------------
//
// (Loading)
// GameBundleRequested
// AtlasCountRequested
// AtlasRequested
//-----------------------

#endregion Events list

using System;
using System.Collections.Generic;

namespace WorldsGame.Utils.EventMessenger
{
    /**
     * A messenger for events that have no parameters.
     */

    static public class Messenger
    {
        private static readonly Dictionary<string, Delegate> eventTable = new Dictionary<string, Delegate>();

        private static void PreOn(string eventType)
        {
            // Create an entry for this event type if it doesn't already exist.
            if (!eventTable.ContainsKey(eventType))
            {
                eventTable.Add(eventType, null);
            }
        }

        static public void On(string eventType, Callback handler)
        {
            // Obtain a lock on the event table to keep this thread-safe.
            lock (eventTable)
            {
                PreOn(eventType);

                // Add the handler to the event.
                eventTable[eventType] = (Callback)eventTable[eventType] + handler;
            }
        }

        static public void On<T>(string eventType, Callback<T> handler)
        {
            // Obtain a lock on the event table to keep this thread-safe.
            lock (eventTable)
            {
                PreOn(eventType);

                // Add the handler to the event.
                eventTable[eventType] = (Callback<T>)eventTable[eventType] + handler;
            }
        }

        static public void On<T1, T2>(string eventType, Callback<T1, T2> handler)
        {
            // Obtain a lock on the event table to keep this thread-safe.
            lock (eventTable)
            {
                PreOn(eventType);

                // Add the handler to the event.
                eventTable[eventType] = (Callback<T1, T2>)eventTable[eventType] + handler;
            }
        }

        static public void On<T1, T2, T3>(string eventType, Callback<T1, T2, T3> handler)
        {
            // Obtain a lock on the event table to keep this thread-safe.
            lock (eventTable)
            {
                PreOn(eventType);

                // Add the handler to the event.
                eventTable[eventType] = (Callback<T1, T2, T3>)eventTable[eventType] + handler;
            }
        }

        static private bool PreOff(string eventType, object handler)
        {
            // If handler is null, purge everything about this event
            if (handler == null)
            {
                eventTable.Remove(eventType);
            }

            // Only take action if this event type exists.
            return eventTable.ContainsKey(eventType);
        }

        private static void PostOff(string eventType)
        {
            // If there's nothing left then remove the event type from the event table.
            if (eventTable[eventType] == null)
            {
                eventTable.Remove(eventType);
            }
        }

        static public void Off(string eventType, Callback handler = null)
        {
            // Obtain a lock on the event table to keep this thread-safe.
            lock (eventTable)
            {
                if (PreOff(eventType, handler))
                {
                    // Remove the event handler from this event.
                    eventTable[eventType] = (Callback)eventTable[eventType] - handler;

                    PostOff(eventType);
                }
            }
        }

        static public void Off<T>(string eventType, Callback<T> handler)
        {
            // Obtain a lock on the event table to keep this thread-safe.
            lock (eventTable)
            {
                if (PreOff(eventType, handler))
                {
                    // Remove the event handler from this event.
                    eventTable[eventType] = (Callback<T>)eventTable[eventType] - handler;

                    PostOff(eventType);
                }
            }
        }

        static public void Off<T1, T2>(string eventType, Callback<T1, T2> handler)
        {
            // Obtain a lock on the event table to keep this thread-safe.
            lock (eventTable)
            {
                if (PreOff(eventType, handler))
                {
                    // Remove the event handler from this event.
                    eventTable[eventType] = (Callback<T1, T2>)eventTable[eventType] - handler;

                    PostOff(eventType);
                }
            }
        }

        static public void Off<T1, T2, T3>(string eventType, Callback<T1, T2, T3> handler)
        {
            // Obtain a lock on the event table to keep this thread-safe.
            lock (eventTable)
            {
                if (PreOff(eventType, handler))
                {
                    // Remove the event handler from this event.
                    eventTable[eventType] = (Callback<T1, T2, T3>)eventTable[eventType] - handler;

                    PostOff(eventType);
                }
            }
        }

        static private Delegate GetEventCallback(string eventType)
        {
            Delegate d;
            lock (eventTable)
            {
                // Invoke the delegate only if the event type is in the dictionary.
                if (eventTable.TryGetValue(eventType, out d))
                {
                    return d;
                }
            }

            return d;
        }

        static public void Invoke(string eventType)
        {
            Callback callback = (Callback)GetEventCallback(eventType);

            // Invoke the delegate if it's not null.
            if (callback != null)
            {
                callback();
            }
        }

        static public void Invoke<T>(string eventType, T arg1)
        {
            Callback<T> callback = (Callback<T>)GetEventCallback(eventType);

            // Invoke the delegate if it's not null.
            if (callback != null)
            {
                callback(arg1);
            }
        }

        static public void Invoke<T1, T2>(string eventType, T1 arg1, T2 arg2)
        {
            Callback<T1, T2> callback = (Callback<T1, T2>)GetEventCallback(eventType);

            // Invoke the delegate if it's not null.
            if (callback != null)
            {
                callback(arg1, arg2);
            }
        }

        static public void Invoke<T1, T2, T3>(string eventType, T1 arg1, T2 arg2, T3 arg3)
        {
            Callback<T1, T2, T3> callback = (Callback<T1, T2, T3>)GetEventCallback(eventType);

            // Invoke the delegate if it's not null.
            if (callback != null)
            {
                callback(arg1, arg2, arg3);
            }
        }

        static public void Clear()
        {
            lock (eventTable)
            {
                eventTable.Clear();
            }
        }
    }
}