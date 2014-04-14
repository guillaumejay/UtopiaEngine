using System;

namespace UE.Core.Architecture.Messages
{
    public class LinkResult
    {
        /// <summary>
        /// Has the link attempt finished ?
        /// </summary>
        public bool IsLinkFinished { get; internal set; }

        public int HitPointLost { get; internal set; }

        public int ComponentLost { get; internal set; }

        /// <summary>
        /// Link attempt has failed : not enough components
        /// </summary>
        public bool HasFailed { get; internal set; }

        public int LinkBox { get; internal set; }

        public bool Connected { get { return IsLinkFinished && !HasFailed; }}

        public string ResultText
        {
            get
            {
                string text=String.Empty;
                if (Connected)
                    text = String.Format("Connection made {0}, ",LinkBox);
                if (HasFailed)
                    text += "Can't link anymore (no more component), ";
                if (ComponentLost>0)
                    text+=String.Format("Lose {0} components, ",ComponentLost);
                return text;
            }
        }
    }
}
