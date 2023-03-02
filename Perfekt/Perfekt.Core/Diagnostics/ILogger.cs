using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Perfekt.Core.Diagnostics
{
    /// <summary>
    /// An interface which allows an object to become a host container to
    /// receive messages.
    /// </summary>
    public interface ILogger
    {
        string Name { get; }

        string Description { get; }

        IEnumerable<IMessageBlock> Messages { get; }
    }

}
