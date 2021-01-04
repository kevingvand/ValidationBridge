using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
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

        static void Main(string[] args)
        {
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

            var modules = Modules.GetLoadedModules();

            //TODO: possibility to get the same instance with different types (i.e. cast the instance over a method)

            //var multimeter = Modules.GetModule("Keithley2000");


            //var multiSource = Modules.CastModule(multimeter, IVoltageSource);

            //IVoltageSource multimeterSource = Modules.CastModule<IVoltageSource>(multimeter);
            //IVoltageSensor multimeterSensor = Modules.CastModule<IVoltageSensor>(multimeter);


            var multimeter = Modules.GetModuleWithType<IVoltageSensor>("Keithley2000");

            multimeter.GetDCVoltage();

            var source = Modules.Cast<IVoltageSource>(multimeter);

            source.SetDCVoltage(123.0);

            multimeter.GetDCVoltage();


            var otherMultimeter = Modules.GetModuleWithType<IVoltageSource>("Keithley2000");

            otherMultimeter.SetDCVoltage(5);

            for (int i = 0; i < 10; i++)
                Console.WriteLine(multimeter.GetDCVoltage());


            Console.WriteLine(multimeter.GetDCVoltage());

            Console.WriteLine(multimeter.GetDescription());

            Console.ReadKey();
        }

        static void PrintModule(IModule module)
        {
            Console.WriteLine(module.GetName() + " " + module.GetDescription());
        }


    }
}
