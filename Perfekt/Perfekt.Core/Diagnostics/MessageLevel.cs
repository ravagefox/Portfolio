namespace Perfekt.Core.Diagnostics
{
    /// <summary>
    /// An object which allows an <see cref="IMessageBlock"/>
    /// to take the form of a given message level.
    /// </summary>
    public enum MessageLevel : byte
    {
        None,
        Debug,
        Info,
        Warning,
        Exception,

        Trace,
        Fatal,
    }
}
