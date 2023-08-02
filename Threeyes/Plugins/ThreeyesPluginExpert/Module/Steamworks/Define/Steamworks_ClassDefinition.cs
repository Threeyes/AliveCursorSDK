using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Threeyes.Steamworks
{

    #region SystemWindow
    public class WindowEventExtArgs : EventArgs
    {
        public Cause cause = Cause.None;
        public StateChange stateChange = StateChange.None;

        public WindowEventExtArgs(Cause cause, StateChange stateChange)
        {
            this.cause = cause;
            this.stateChange = stateChange;
        }

        /// <summary>
        /// Why this event get fired
        /// </summary>
        public enum Cause
        {
            None = 0,

            ResolutionChanged = 1 << 0,//Resolution has changed
            MonitorChanged = 1 << 2,//App has been moved to other Monitor

            All = ~0
        }

        /// <summary>
        /// Enumeration specifying a change in CursorAppearance state
        /// </summary>
        public enum StateChange
        {
            None = 0,

            Before = 1 << 0,
            //Processing = 1 << 1,
            After = 1 << 2,

            All = ~0
        }
    }
}


#endregion