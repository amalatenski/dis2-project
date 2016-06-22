using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Test
{
    class HandEventArgs : EventArgs
    {
        //Placeholderobject for application
        static Object Application;

        public enum HandSide
        {
            Left,
            Right,
        };

        public enum HandState
        {
            Unknown,
            NotTracked,
            Open,
            Closed,
            Lasso,
        };

        public HandSide agent;

        public HandState leftHandState;
        public HandState rightHandState;

        public Int32 leftHandX, leftHandY, leftHandZ;
        public Int32 rightHandX, rightHandY, rightHandZ;

        public HandEventArgs(HandSide agent, HandState leftHandState, Int32 leftHandX, Int32 leftHandY, Int32 leftHandZ, HandState rightHandState, Int32 rightHandX, Int32 rightHandY, Int32 rightHandZ)
        {
            this.agent = agent;
            this.leftHandState = leftHandState;
            this.leftHandX = leftHandX;
            this.leftHandY = leftHandY;
            this.leftHandZ = leftHandZ;
            this.rightHandState = rightHandState;
            this.rightHandX = rightHandX;
            this.rightHandY = rightHandY;
            this.rightHandZ = rightHandZ;
        }

        //getter methods for the data of the hand specified by the evetns agent
        public HandState getHandState()
        {
            if (agent == HandSide.Left) { return leftHandState; }
            return rightHandState;
        }

        public Int32 getHandX()
        {
            if (agent == HandSide.Left) { return leftHandX; }
            return rightHandX;
        }

        public Int32 getHandY()
        {
            if (agent == HandSide.Left) { return leftHandY; }
            return rightHandY;
        }

        public Int32 getHandZ()
        {
            if (agent == HandSide.Left) { return leftHandZ; }
            return rightHandZ;
        }
    }
}
