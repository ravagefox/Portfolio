namespace Perfekt.Core.Diagnostics
{
    /// <summary>
    /// Defines a message that contains the basic information used when
    /// submitting a message.
    /// </summary>
    public struct MessageValue : IMessageBlock
    {
        public int ExceptionId { get; }

        public string Message { get; }

        public MessageLevel MessageType { get; }


        public MessageValue(string lMessage)
            : this(lMessage, (byte)MessageLevel.None, 0)
        {
        }

        public MessageValue(string lMsg, byte lMsgLvl, int lErr)
        {
            this.Message = lMsg;
            this.MessageType = (MessageLevel)lMsgLvl;
            this.ExceptionId = lErr;
        }


        public override string ToString()
        {
            return $"Message Level: {this.MessageType}, Message: {this.Message}";
        }
    }
}
