using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.IO.Pipes;
using System.Linq;
using System.Text;
using System.Threading;
using ValidationBridge.Bridge.Adapters.Python.Enumerations;
using ValidationBridge.Bridge.Adapters.Python.Messages;
using ValidationBridge.Common;
using ValidationBridge.Common.Enumerations;

namespace ValidationBridge.Bridge.Adapters.Python
{
    public class PythonConsole
    {
        public const string DefaultPythonPath = "python";

        private string _pythonPath;
        private string _serverScript;
        private NamedPipeServerStream _server;
        private BinaryReader _reader;
        private BinaryWriter _writer;

        public string Id { get; set; }
        public Thread ConsoleThread { get; set; }

        public PythonConsole(string id, string pythonPath = DefaultPythonPath)
        {
            Id = id;

            _pythonPath = DefaultPythonPath;
            _server = new NamedPipeServerStream(Id, PipeDirection.InOut, 1);
            _reader = new BinaryReader(_server);
            _writer = new BinaryWriter(_server);
        }

        public void Start()
        {
            if (_serverScript == null)
                Build();

            ConsoleThread = new Thread(() =>
            {
                RunPythonCommand($@"-c ""{_serverScript}""");
            });
            ConsoleThread.Start();

            _server.WaitForConnection();
        }

        public void Stop()
        {
            _server?.Close();
            ConsoleThread?.Abort();
        }

        public bool IsInstalled(bool ignoreException = true)
        {
            try
            {
                GetVersion();
            }
            catch (PythonException e)
            {
                if (!ignoreException) throw e;
                return false;
            }

            return true;
        }

        public string GetVersion()
        {
            var result = RunPythonCommand("--version");
            var lines = result.Split(new string[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);

            return lines.Last();
        }

        public void Execute(string command)
        {
            var message = new PythonTextMessage(EPythonMessageType.EXECUTE, command);
            _writer.Write(message.GetBytes());
            ReadMessage();
        }

        public dynamic Evaluate(string expression)
        {
            var message = new PythonTextMessage(EPythonMessageType.EVALUATE, expression);
            _writer.Write(message.GetBytes());
            var resultMessage = ReadMessage();

            return resultMessage.GetValue();
        }

        public void Import(params string[] modules)
        {
            Execute($"import {string.Join(", ", modules)}");
        }

        public void AddPath(string path)
        {
            Import("sys");
            Execute($"sys.path.append('{GetFormattedPath(path)}')");
        }

        public void DefineVariable(string variableName, dynamic expression)
        {
            Execute($"{variableName} = {expression}");
        }

        public void DefineStringVariable(string variableName, string value) => DefineVariable(variableName, $@"'{value}'");

        public string GetFormattedPath(string path)
        {
            return new PythonBuilder().GetFormattedPath(path);
        }

        private string RunPythonCommand(string command)
        {
            ProcessStartInfo start = new ProcessStartInfo
            {
                FileName = _pythonPath,
                Arguments = command,
                UseShellExecute = false,
                CreateNoWindow = true,
                RedirectStandardOutput = true,
                RedirectStandardError = true
            };

            try
            {
                using (Process process = Process.Start(start))
                using (StreamReader reader = process.StandardOutput)
                {
                    string error = process.StandardError.ReadToEnd();

                    if (error.Length > 0)
                        throw new PythonException(error);

                    return reader.ReadToEnd();
                }
            }
            catch (Win32Exception)
            {
                throw new PythonException($"Could not start Python, make sure it is configured correctly (PATH={_pythonPath})!");
            }
        }

        private string GetEncoding()
        {
            var encoding = Constants.ServerEncoding;
            if (encoding.GetType() == typeof(UnicodeEncoding)) return "unicode_escape";
            if (encoding.GetType() == typeof(ASCIIEncoding)) return "ascii";
            if (encoding.GetType() == typeof(UTF32Encoding)) return "utf-32";
            return "utf-8";
        }

        private void Build()
        {
            var builder = new PythonBuilder();

            // Add needed imports. Struct is needed for byte conversion and time for delays (sleep).
            builder.AddImports("struct");
            builder.AddImports("time");

            // Set global variables, these are retrieved from the environment
            builder.DefineStringVariable("pipe_name", Id);
            builder.DefineStringVariable("encoding", GetEncoding());

            // Connect function definition, attempt to connect every 100ms. If no connection was established after a 100 times, the script terminates
            builder.AddLine("def connect(path, attempt):");
            builder.AddLine("try:", 1);
            builder.AddLine("if attempt > 100:", 2);
            builder.AddLine("return", 3);
            builder.DefineVariable("server", "open(path, 'r+b', 0)", 2);
            builder.AddLine("server.seek(0)", 2);
            builder.AddLine("return server", 2);
            builder.AddLine("except:", 1);
            builder.AddLine("time.sleep(.1)", 2);
            builder.AddLine("connect(path, attempt + 1)", 2);

            // write_message function definition, write the message and flush the buffers
            builder.AddLine("def write_message(pipe, message):");
            builder.AddLine("pipe.write(message)", 1);
            builder.AddLine("pipe.seek(0)", 1);

            // read_message function definition, read the message from the server and pass it on to the process_message function
            builder.AddLine("def read_message(pipe):");
            builder.DefineVariable("type_bytes", "pipe.read(1)", 1);
            builder.AddLine("if not type_bytes.__len__() is 1:", 1);
            builder.AddLine("pipe.close()", 2);
            builder.AddLine("return", 2);
            builder.DefineVariable("length", "struct.unpack('l', pipe.read(4))[0]", 1);
            builder.DefineVariable("message", "pipe.read(length).decode(encoding)", 1);
            builder.AddLine("pipe.seek(0)", 1);
            builder.AddLine("process_message(pipe, type_bytes[0], message)", 1);

            // get_length_bytes function definition, get the byte array representation of a length
            builder.AddLine("def get_length_bytes(length):");
            builder.AddLine("return struct.pack('l', length)", 1);

            // write_error function definition, packs an error message and writes it back to the server
            builder.AddLine("def write_error(pipe, error):");
            builder.DefineVariable("error_message", "error.encode(encoding)", 1);
            builder.DefineVariable("error_bytes", "struct.pack('b', 4) + get_length_bytes(error_message.__len__()) + error_message", 1);
            builder.AddLine("write_message(pipe, error_bytes)", 1);

            // read_array function definition, parses an array and packs it in a byte array in a readable format for the pipe server
            builder.AddLine("def read_array(result):");
            builder.DefineVariable("array_length", "result.__len__()", 1);
            builder.AddLine("if array_length is 0:", 1);
            builder.AddLine("return [bytes(0), 32]", 2);
            builder.DefineVariable("value_bytes, result_type", "get_bytes(result[0])", 1);
            builder.DefineVariable("value_bytes", "get_length_bytes(value_bytes.__len__()) + value_bytes", 1);
            builder.AddLine("for i in range(1, array_length):", 1);
            builder.DefineVariable("element_bytes", "get_bytes(result[i])[0]", 2);
            builder.AddLine("value_bytes += get_length_bytes(element_bytes.__len__()) + element_bytes", 2);
            builder.DefineVariable("value_bytes", "get_length_bytes(array_length) + value_bytes", 1);
            builder.AddLine("return [value_bytes, 32 | result_type]", 1);

            // get_bytes function definition, parses any value and packs it in a byte array in a readable format for the pipe server
            builder.AddLine("def get_bytes(value):");
            builder.AddLine("if isinstance(value, list):", 1);
            builder.DefineVariable("value_bytes, result_type", "read_array(value)", 2);
            builder.AddLine("elif isinstance(value, float):", 1);
            builder.DefineVariable("value_bytes", "struct.pack('d', value)", 2);
            builder.DefineVariable("result_type", "4", 2);
            builder.AddLine("elif type(value) is bool:", 1);
            builder.DefineVariable("value_bytes", "struct.pack('?', value)", 2);
            builder.DefineVariable("result_type", "2", 2);
            builder.AddLine("elif isinstance(value, int):", 1);
            builder.DefineVariable("value_bytes", "struct.pack('i', value)", 2);
            builder.DefineVariable("result_type", "1", 2);
            builder.AddLine("else:", 1);
            builder.DefineVariable("value_bytes", "str(value).encode(encoding)", 2);
            builder.DefineVariable("result_type", "16", 2);
            builder.AddLine("return [value_bytes, result_type]", 1);

            // evaluate_message function definition, evaluates the input, packs the result in a byte array and sends it back to the server
            builder.AddLine("def evaluate_message(pipe, message):");
            builder.AddLine("try:", 1);
            builder.DefineVariable("result", "eval(message, globals())", 2);
            builder.AddLine("except Exception as e:", 1);
            builder.AddLine("write_error(pipe, str(e))", 2);
            builder.AddLine("return", 2);
            builder.DefineVariable("value_bytes, result_type", "get_bytes(result)", 1);
            builder.DefineVariable("result_bytes", "struct.pack('B B', 3, result_type) + get_length_bytes(value_bytes.__len__()) + value_bytes", 1);
            builder.AddLine("write_message(pipe, result_bytes)", 1);

            // execute_message function definition, executes the input, and packs the value "true" in a byte array if execution was successful - otherwise the write_error function is called
            builder.AddLine("def execute_message(pipe, message):");
            builder.AddLine("try:", 1);
            builder.AddLine("exec(message, globals())", 2);
            builder.AddLine("except Exception as e:", 1);
            builder.AddLine("write_error(pipe, str(e))", 2);
            builder.AddLine("return", 2);
            builder.AddLine("write_message(pipe, struct.pack('B B', 3, 2) + struct.pack('l', 1) + struct.pack('?', True))", 1);

            // process_message function definition, terminates if the server is unreachable and decides whether the input should be executed or evaluated.
            builder.AddLine("def process_message(pipe, type, message):");
            builder.AddLine("if pipe.closed:", 1);
            builder.AddLine("return", 2);
            builder.AddLine("if type is 1:", 1);
            builder.AddLine("evaluate_message(pipe, message)", 2);
            builder.AddLine("if type is 2:", 1);
            builder.AddLine("execute_message(pipe, message)", 2);

            // Main loop of the process, connects to the server and runs until the server shuts down, otherwise processes the input sequentially
            builder.AddLine("if __name__ == '__main__':");
            builder.DefineVariable("pipeRoot", @"R'\\.\Pipe'", 1);
            builder.DefineVariable("pipePath", @"pipeRoot + '\\' + pipe_name", 1);
            builder.DefineVariable("server", "connect(pipePath, 0)", 1);
            builder.AddLine("if server is None:", 1);
            builder.AddLine("exit(0)", 2);
            builder.AddLine("while True:", 1);
            builder.AddLine("if server.closed:", 2);
            builder.AddLine("exit(0)", 3);
            builder.AddLine("read_message(server)", 2);

            _serverScript = builder.Script;
        }

        private PythonResultMessage ReadMessage()
        {
            var messageType = (EPythonMessageType)_reader.ReadByte();
            if (messageType == EPythonMessageType.ERROR)
            {
                var length = _reader.ReadInt32();
                var errorMessage = Constants.ServerEncoding.GetString(_reader.ReadBytes(length));
                throw new PythonException(errorMessage);
            }
            else if (messageType == EPythonMessageType.RESULT)
            {
                var resultType = (EType)_reader.ReadByte();
                var length = _reader.ReadInt32();

                return new PythonResultMessage(resultType, _reader.ReadBytes(length));
            }
            else
            {
                return null;
            }
        }
    }
}
