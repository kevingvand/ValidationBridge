using System;
using System.Collections.Generic;
using System.Text;

namespace ValidationBridge.Common.Interfaces.Modules
{
    /// <summary>
    /// Base interface for every module, containing basic module data
    /// </summary>
    public interface IModule
    {
        /// <summary>
        ///  The Unique Identifier of the module instance
        /// </summary>
        Guid InstanceId { get; }

        /// <summary>
        /// Retrieves the human readable name for the module
        /// </summary>
        string GetName();

        /// <summary>
        ///  Retrieves a description of the content for the module
        /// </summary>
        string GetDescription();
    }
}
