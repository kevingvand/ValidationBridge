using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace ValiationBridge.Bridge.Adapters.Matlab
{
    public class Matlab
    {
        private Type _type;
        private object _app;

        public Matlab(bool isVisible = false)
        {
            _type = Type.GetTypeFromProgID("matlab.application");
            _app = Activator.CreateInstance(_type);
            SetVisible(true); 
        }

        public static bool IsInstalled()
        {
            return Type.GetTypeFromProgID("matlab.application") != null;
        }

        public void SetVisible(bool isVisible)
        {
            _type.InvokeMember("Visible", BindingFlags.Instance |BindingFlags.Public | BindingFlags.SetProperty, Type.DefaultBinder, _app, new object[] { isVisible });
        }

        public void Execute(string command)
        {
            _type.InvokeMember("Execute", BindingFlags.InvokeMethod, null, _app, new object[] { command });
        }

        public dynamic GetVariable(string name)
        {
            return _type.InvokeMember("GetVariable", BindingFlags.InvokeMethod, null, _app, new object[] { name, "base" });
        }

        public TType GetVariable<TType>(string name)
        {
            return (TType)GetVariable(name, typeof(TType));
        }

        public dynamic GetVariable(string name, Type type)
        {
            if (type.IsArray)
            {
                Type elementType = type.GetElementType();
                return ParseArray(name, elementType);
            }

            return Convert.ChangeType(GetVariable(name), type);
        }

        public void SetVariable(string name, dynamic value)
        {
            _type.InvokeMember("PutWorkspaceData", BindingFlags.InvokeMethod, null, _app, new object[] { name, "base", value });
        }

        public dynamic EvaluateExpression(string variableName, string expression, bool clearVariable = false)
        {
            Execute($"{variableName} = {expression}");
            var value = GetVariable(variableName);

            if (clearVariable)
                Execute($"clear {variableName}");

            return value;
        }

        public void AddPath(string path)
        {
            Execute($"addpath '{path}'");
        }

        private dynamic ParseArray(string name, Type arrayType)
        {
            Execute($"[{name}_rows, {name}_cols] = size({name})");
            int rows = (int)GetVariable($"{name}_rows");
            int cols = (int)GetVariable($"{name}_cols");

            dynamic result;

            if (rows == 1 && cols == 1)
            {
                result = Array.CreateInstance(arrayType, 1);
                result.SetValue(GetVariable(name), 0);
            }
            else if (rows == 1)
            {
                result = Array.CreateInstance(arrayType, cols);

                for (int i = 0; i < cols; i++)
                    result.SetValue(Convert.ChangeType(EvaluateExpression($"{name}_i", $"{name}(1, {i + 1});"), arrayType), i);
            }
            else if (cols == 1)
            {
                result = Array.CreateInstance(arrayType, rows);

                for (int i = 0; i < rows; i++)
                    result.SetValue(Convert.ChangeType(EvaluateExpression($"{name}_i", $"{name}({i + 1}, 1);"), arrayType), i);
            }
            else
            {
                result = Array.CreateInstance(arrayType, rows, cols);

                for (int row = 0; row < rows; row++)
                    for (int col = 0; col < cols; col++)
                        result.SetValue(Convert.ChangeType(EvaluateExpression($"{name}_i", $"{name}({row + 1}, {col + 1});"), arrayType), row, col);
            }

            return result;
        }
    }
}
