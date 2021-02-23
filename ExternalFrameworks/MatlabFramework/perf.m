tic
% Important for generic method: https://www.mathworks.com/help/matlab/matlab_external/call-net-generic-methods.html
cd 'C:\Users\Asus\Documents\Development\ValidationBridge\ValidationBridge\ExternalFrameworks\MatlabFramework'
projectDir = fileparts(fileparts(pwd));
invokerPath = fullfile(projectDir, 'ValidationBridge.Invoker', 'bin', 'Debug', 'ValidationBridge.Invoker.dll');
commonPath = fullfile(projectDir, 'ValidationBridge.Common', 'bin', 'Debug', 'netstandard2.0', 'ValidationBridge.Common.dll');
NET.addAssembly(invokerPath);
NET.addAssembly(commonPath);

IPlotter = 'ValidationBridge.Common.Interfaces.Modules.IPlotter';
ValidationBridge.Invoker.Modules.LoadModule("C:\Users\Asus\PycharmProjects\ValidationBridgeFramework\Modules")

module = NET.invokeGenericMethod('ValidationBridge.Invoker.Modules', 'GetModuleWithType', {IPlotter}, 'Keithley2001');

double(module.GetPoints(5))

toc

%%
tic
double(module.GetPoints(100))
toc