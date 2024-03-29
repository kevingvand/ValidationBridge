﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using InstrumentModules;
using ValidationBridge.Common.Interfaces.Modules;
using ValidationBridge.Invoker;

namespace ConsoleFramework
{

    class Program
    {
        static List<MemberInfo> GetMemberInfo(Type type)
        {
            var members = type.GetMembers().ToList();
            var interfaces = type.GetInterfaces();

            foreach (var moduleInterface in interfaces)
                members.AddRange(moduleInterface.GetMembers());

            return members;
        }

        //public delegate TResult Func<in T1, in T2, out TResult>(T1 arg1, T2 arg2);

        public static string CSharpModulePath = @"C:\Users\Asus\Documents\Development\ValidationBridge\ValidationBridge\InstrumentModules\bin\Debug\InstrumentModules.dll";
        public static string MATLABModulePath = @"C:\Users\Asus\Documents\Uni\THESIS\Benchmark\Modules\MATLAB\+Modules";
        public static string PythonModulePath = @"C:\Users\Asus\Documents\Uni\THESIS\Benchmark\Modules\Python";

        static void Main(string[] args)
        {
            Modules.LoadModule(CSharpModulePath);
            Modules.LoadModule(MATLABModulePath);
            Modules.LoadModule(PythonModulePath);

            var modules = Modules.GetLoadedModules();

            //Console.ReadKey();
            var stopWatch = new Stopwatch();
            var loadedModules = Modules.LoadModule(@"C:\Users\Asus\Documents\Development\ValidationBridge\ValidationBridge\InstrumentModules\bin\Debug\InstrumentModules.dll");

            var csharpBenchmark = Modules.GetModuleWithType<IBenchmark>("CSharpBenchmark");
            var csharpDirect = new CsharpBenchmark();

            //stopWatch.Restart();
            //for(int i = 0; i < 1000; i++)
            //    csharpBenchmark.Add(i, 5);
            //stopWatch.Stop();
            //Console.WriteLine("Elapsed (ms): " + stopWatch.ElapsedMilliseconds + " - Result: ");

            //stopWatch.Restart();
            //for (int i = 0; i < 1000; i++)
            //    csharpDirect.Add(i, 5);
            //stopWatch.Stop();
            //Console.WriteLine("Elapsed (ms): " + stopWatch.ElapsedMilliseconds + " - Result: ");


            var input = Enumerable.Range(1, 5000).ToArray();

            stopWatch.Restart();
            var res = csharpBenchmark.DelayedAddToArray(input, 5);
            //var res = csharpBenchmark.Add(8, 2);
            stopWatch.Stop();

            Console.Write(("Elapsed (ms):" + stopWatch.ElapsedMilliseconds));

            Debugger.Break();


            //var a = new
            //{
            //    Name = "Hello",
            //    Description = "Some description",
            //    GetDCVoltage = new Func<double>(() => { return 4.2; }),
            //    GetACVoltage = new Func<double>(() => { return 1.4; }),
            //};

            //var y = a.ActLike<IVoltageSensor>();
            //var qq = y.GetDCVoltage();

            /*
             * ON INVOKE, THE FOLLOWING INFO IS NEEDED:
             *  - ID OF THE INVOKED OBJECT
             *  - NAME OF THE INVOKED METHOD
             *  - VALUEs OF THE PASSED VARIABLES
             */

            //TODO: check void methods

            //ModuleInvoker invoker = new ModuleInvoker();
            //List<MemberInfo> moduleInfo = GetMemberInfo(typeof(IVoltageSensor));


            //foreach (var info in moduleInfo)
            //{
            //    if (info.MemberType == MemberTypes.Method)
            //    {
            //        var methodInfo = (MethodInfo)info;

            //        var parameterCount = methodInfo.GetParameters().Count();

            //        var proxyBody = new ObjectFunc((proxyParameters) =>
            //        {
            //            //TODO: Bridge communication

            //            //TODO: move to Invoker assembly

            //            /*
            //            * For each method the following sequence should happen:
            //            * 1- Summarize the method in a way it can be send over pipes and understood by the bridge
            //            * 2- Send the command over the bridge
            //            * 3- Wait for a response from the bridge
            //            * 4- Convert response to fit the return value and return.
            //            */

            //            return null;
            //        });

            //        var proxyInputs = new ParameterExpression[parameterCount];

            //        for (int i = 0; i < parameterCount; i++)
            //            proxyInputs[i] = Expression.Parameter(typeof(object));

            //        var proxyCallExpression = Expression.Call(Expression.Constant(proxyBody.Target), proxyBody.Method, Expression.NewArrayInit(typeof(object), proxyInputs));
            //        invoker.AddMember(info.Name, Expression.Lambda(proxyCallExpression, proxyInputs).Compile());
            //    }
            //    else if (info.MemberType == MemberTypes.Property)
            //    {
            //        //TODO: some way to redirect get and set?
            //        invoker.AddMember(info.Name, info);
            //    }
            //}

            //var z = invoker.Instance;
            //var df = invoker.Instance.ActLike<IVoltageSensor>();

            //df.Blabla(5, 6);

            //Console.ReadKey();


            //while (true)
            //{
            //    var message = Console.ReadLine();

            //    if (message.ToLower().StartsWith("q")) return;

            //    if (message.ToLower().Equals("modules"))
            //    {
            //        var modules = Modules.GetLoadedModules();
            //        Console.WriteLine(string.Join(Environment.NewLine, modules));
            //    }

            //    if(message.ToLower().StartsWith("module"))
            //    {
            //        var command = message.Split(' ');
            //        if(command.Length < 2)
            //        {
            //            Console.WriteLine("Please specify a module name");
            //        }
            //    }
            //}


            //TODO: possibility to get the same instance with different types (i.e. cast the instance over a method)

            //var multimeter = Modules.GetModule("Keithley2000");


            //var multiSource = Modules.CastModule(multimeter, IVoltageSource);

            //IVoltageSource multimeterSource = Modules.CastModule<IVoltageSource>(multimeter);
            //IVoltageSensor multimeterSensor = Modules.CastModule<IVoltageSensor>(multimeter);

            //var loadedModules = Modules.LoadModule(@"C:\Users\Asus\Documents\Development\ValidationBridge\ValidationBridge\InstrumentModules\bin\Debug\InstrumentModules.dll");
            //var matlabModules = Modules.LoadModule(@"C:\Users\Asus\Documents\Development\ValidationBridge\ValidationBridge\ExternalModules\+Instruments");
            //var pythonModules = Modules.LoadModule(@"C:\Users\Asus\PycharmProjects\ValidationBridgeFramework\Modules");

            //var modules = Modules.GetLoadedModules();

            //var module = Modules.GetModule("Keithley2001");

            //var plotter = Modules.Cast<IPlotter>(module);

            //var z = plotter.GetPoints(100);

            ////var test = Modules.GetModuleWithType<IVoltageSource>("Keithley2000");

            ////var module = Modules.GetModule("KeysightB2901A2");
            //var source = Modules.Cast<IVoltageSource>(module);
            //var sensor = Modules.Cast<IVoltageSensor>(module);

            //var x = source.GetDescription();

            //var val1 = sensor.GetDCVoltage();
            //source.SetDCVoltage(14);
            //var val2 = sensor.GetDCVoltage();

            //var test = Modules.GetModuleWithType<IVoltageSensor>("KeysightB2901A2");

            //var x = test.GetDCVoltage();

            //var test2 = Modules.Cast<IVoltageSource>(test);
            //test2.SetDCVoltage(10);
            //var y = test.GetDCVoltage();

            //var multimeter = Modules.GetModuleWithType<IVoltageSensor>("Keithley2400");
            ////var aa = multimeter.ConnectGPIB(0, 9);
            //var aa = multimeter.Connect("ssPIB");

            //var ab = multimeter.GetDCVoltage();

            //var source = Modules.Cast<IVoltageSource>(multimeter);

            //source.SetDCVoltage(123.5);

            //multimeter.GetDCVoltage();


            //var otherMultimeter = Modules.GetModuleWithType<IVoltageSource>("Keithley2400");

            //otherMultimeter.SetDCVoltage(5);

            //for (int i = 0; i < 10; i++)
            //    Console.WriteLine(multimeter.GetDCVoltage());


            //Console.WriteLine(multimeter.GetDCVoltage());

            //Console.WriteLine(multimeter.GetDescription());

            Console.ReadKey();
        }

        static void PrintModule(IModule module)
        {
            Console.WriteLine(module.GetName() + " " + module.GetDescription());
        }


    }
}
