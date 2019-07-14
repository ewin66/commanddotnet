﻿using System.Collections.Generic;
using System.Reflection;
using CommandDotNet.Execution;

namespace CommandDotNet.ClassModeling.Definitions
{
    internal interface IMethodDef : IInvocation
    {
        IReadOnlyCollection<IArgumentDef> ArgumentDefs { get; }
        MethodBase MethodBase { get; }
    }
}