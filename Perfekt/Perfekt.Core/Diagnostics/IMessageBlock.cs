using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Perfekt.Core.Diagnostics
{
    /// <summary>
    /// An interface which exposes base properties for messages that are received
    /// by a <see cref="ILogger"/> instance.
    /// </summary>
    public interface IMessageBlock
    {
        int ExceptionId { get; }
        string Message { get; }

        MessageLevel MessageType { get; }
    }
}
