namespace Perfekt.Core.Diagnostics
{

    /// <summary>
    /// Container for holding details about <see cref="IMessageBlock"/> that 
    /// are posted.
    /// </summary>
    public class MessageLoggedEventArgs : EventArgs
    {
        public IMessageBlock Message { get; }


        public MessageLoggedEventArgs(IMessageBlock message)
        {
            this.Message = message;
        }
    }
}