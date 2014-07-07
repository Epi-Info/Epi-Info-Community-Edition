using System;
using System.Collections.Generic;
using System.Text;

namespace Epi.EnterCheckCodeEngine
{
    public class RunCheckCodeEventArgs : EventArgs
    {

        EventActionEnum mEventType;
        string mParameter;
        public RunCheckCodeEventArgs(EventActionEnum pEventType, string pParameter)
        {
            mEventType = pEventType;
            mParameter = pParameter;
        }

        public EventActionEnum EventType
        {
            get { return mEventType; }
        }

        public string Parameter
        {
            get { return mParameter; }
        }

    }
}
