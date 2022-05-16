using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ValidationBridge.Common.Interfaces.Modules;

namespace InstrumentModules
{
    [Export(typeof(IModule))]
    public class CsharpBenchmark : IBenchmark
    {
        public Guid InstanceId => Guid.Empty;

        public string GetName() => "CSharpBenchmark";

        public string GetDescription() => "Module for testing the performance of C# modules";

        public int Add(int a, int b)
        {
            return a + b;
        }

        public int DelayedAdd(int a, int b)
        {
            Thread.Sleep(10);
            return Add(a, b);
        }

        public int[] AddToArray(int[] array, int value)
        {
            for (var i = 0; i < array.Length; i++)
                array[i] += value;

            return array;
        }

        public int[] DelayedAddToArray(int[] array, int value)
        {
            Thread.Sleep(10);
            return AddToArray(array, value);
        }
    }
}
