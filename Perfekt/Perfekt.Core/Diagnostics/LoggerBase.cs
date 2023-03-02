using System.Diagnostics;
using System.Text;

namespace Perfekt.Core.Diagnostics
{

#pragma warning disable CS8618 // Disable the Nullable reference message


    public abstract class LoggerBase : ILogger
    {
        #region Nested Types
        /// <summary>
        /// Used to automatically collect timestamped information caused when
        /// the logger has received a message.
        /// </summary>
        private struct TimeStampedMessage : IMessageBlock
        {
            public string Message { get; }
            public DateTime TimeStamp { get; }
            public MessageLevel MessageType { get; }
            public int ExceptionId { get; }


            public TimeStampedMessage(string lMsgStr, byte lMsg, int lErr)
            {
                this.Message = lMsgStr;
                this.TimeStamp = DateTime.Now;
                this.MessageType = (MessageLevel)lMsg;
                this.ExceptionId = lErr;
            }


            public static TimeStampedMessage FromMessageValue(IMessageBlock lMsg)
            {
                // Copy the values from the message into a timestamped value.
                return new TimeStampedMessage(
                       lMsg.Message,
                       (byte)lMsg.MessageType,
                       lMsg.ExceptionId);
            }
        }
        #endregion

        #region Properties
        public string Name { get; protected set; }

        public IEnumerable<IMessageBlock> Messages => this.m_msgs;

        public string Description { get; set; }
        #endregion

        #region Fields
        public event EventHandler<MessageLoggedEventArgs> MessageLogged;


        protected List<IMessageBlock> m_msgs;
        private readonly List<TimeStampedMessage> m_tstampMsgs;
        #endregion

        #region Constructors

        protected LoggerBase(string lName, string lDescription)
        {
            if (lName is null) { throw new ArgumentNullException(nameof(lName)); }

            this.Name = lName;
            this.Description = lDescription;
            this.m_msgs = new List<IMessageBlock>();
            this.m_tstampMsgs = new List<TimeStampedMessage>();
        }
        #endregion

        #region Public Methods
        public void WriteMessage(string lMsgStr, byte lMsgLvl, int lErr)
        {
            var lMsg = new MessageValue(lMsgStr, lMsgLvl, lErr);
            this.m_msgs.Add(lMsg);

            // Capture a timestamped message for log saving later.
            this.m_tstampMsgs.Add(TimeStampedMessage.FromMessageValue(lMsg));

            this.OnMessageLogged(this, new MessageLoggedEventArgs(lMsg));
        }

        public void WriteInfo(string lMsgStr)
        {
            this.WriteMessage(lMsgStr, (byte)MessageLevel.Info, -1);
        }

        public void WriteException(Exception lEx, bool lDetailed)
        {
            this.WriteMessage(lDetailed ? lEx.ToString() : lEx.Message, (byte)MessageLevel.Exception, lEx.HResult);
        }


        public void SaveLog(Stream lDst)
        {
            using (var lWrit = new StreamWriter(lDst, Encoding.UTF8))
            {
                foreach (var lMsg in this.m_tstampMsgs)
                {
                    // Format the message for friendly viewing of the log.
                    var fmt = $"[{lMsg.TimeStamp:dd/MM/yy HH:mm:ss}] {lMsg.MessageType}: {lMsg.Message}";
                    lWrit.WriteLine(fmt);
                }
            }
        }
        #endregion

        #region Protected Virtual Methods
        protected virtual void OnMessageLogged(object sender, MessageLoggedEventArgs e)
        {
            MessageLogged?.Invoke(sender, e);
        }
        #endregion

    }
}
