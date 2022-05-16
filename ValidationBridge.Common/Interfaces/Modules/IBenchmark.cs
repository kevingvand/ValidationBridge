using System;
using System.Collections.Generic;
using System.Text;

namespace ValidationBridge.Common.Interfaces.Modules
{
    public interface IBenchmark : IModule
    {
        int Add(int a, int b);
        int DelayedAdd(int a, int b);
        int[] AddToArray(int[] array, int value);
        int[] DelayedAddToArray(int[] array, int value);
    }
}
