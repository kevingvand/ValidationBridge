﻿using System;
using System.Collections.Generic;
using System.Text;

namespace ValidationBridge.Common.Interfaces.Modules
{
    public interface IModule
    {
        string Name { get; }
        string Description { get; }
    }
}