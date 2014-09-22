using System;
using System.Collections.Generic;
using System.Text;
using EpiInfo.Plugin;

namespace Epi.EnterCheckCodeEngine
{
    public class OpenViewEventArgs : EventArgs
    {

        IEnterInterpreterHost mEnterCheckCodeInterface;
        int mViewId;

        public OpenViewEventArgs(IEnterInterpreterHost pEnterCheckCodeInterface, int pViewId)
        {
            mEnterCheckCodeInterface = pEnterCheckCodeInterface;
            mViewId = pViewId;
        }

        public IEnterInterpreterHost EnterCheckCodeInterface
        {
            get { return mEnterCheckCodeInterface; }
        }

        public int ViewId
        {
            get { return mViewId; }
        }

    }

}
