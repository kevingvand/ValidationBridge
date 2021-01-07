% Important for generic method: https://www.mathworks.com/help/matlab/matlab_external/call-net-generic-methods.html
cd 'C:\Users\Asus\Documents\Development\ValidationBridge\ValidationBridge\ExternalFrameworks\MatlabFramework'
projectDir = fileparts(fileparts(pwd));
invokerPath = fullfile(projectDir, 'ValidationBridge.Invoker', 'bin', 'Debug', 'ValidationBridge.Invoker.dll');
commonPath = fullfile(projectDir, 'ValidationBridge.Common', 'bin', 'Debug', 'netstandard2.0', 'ValidationBridge.Common.dll');
NET.addAssembly(invokerPath);
NET.addAssembly(commonPath);

IVoltageSensorType = 'ValidationBridge.Common.Interfaces.Modules.IVoltageSensor';
IVoltageSourceType = 'ValidationBridge.Common.Interfaces.Modules.IVoltageSource';
%%
loadedModules = ValidationBridge.Invoker.Modules.GetLoadedModules();
disp(loadedModules.Item(0))

%%

x = ValidationBridge.Invoker.Modules.GetModule("Keithley2000")

%%

ret = NET.invokeGenericMethod('ValidationBridge.Invoker.Modules', 'GetModuleWithType', {IVoltageSensorType}, 'Keithley2000')

%% NOT WORKING...
ret = NET.invokeGenericMethod('ValidationBridge.Invoker.Modules', 'Cast', {IVoltageSensorType}, x)

%%
ret = NET.invokeGenericMethod('ValidationBridge.Invoker.Modules', 'GetTest', {IVoltageSensorType}, x.InstanceId.ToString)

%%

testPath = fullfile('C:\Users\Asus\source\repos\MatlabCodeGenTest\MatlabCodeGenTest\bin\Debug', 'MatlabCodeGenTest.dll');
NET.addAssembly(testPath);
%%
test = MatlabCodeGenTest.Entry;
test.GetTest()