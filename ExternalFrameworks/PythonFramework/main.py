# This is a sample Python script.

# Press Shift+F10 to execute it or replace it with your code.
# Press Double Shift to search everywhere for classes, files, tool windows, actions, and settings.
import clr
import sys
sys.path.append('C:/Users/Asus/Documents/Development/ValidationBridge/ValidationBridge/ValidationBridge.Invoker/bin/Debug')
clr.AddReference("ValidationBridge.Invoker")
clr.AddReference("ValidationBridge.Common")
from ValidationBridge.Invoker import Modules
from ValidationBridge.Common.Interfaces.Modules import IVoltageSource
from ValidationBridge.Common.Interfaces.Modules import IVoltageSource

# Press the green button in the gutter to run the script.
if __name__ == '__main__':

    x = Modules.GetLoadedModules()
    a = Modules.GetModule("Keithley2000")

    source = Modules.Cast[IVoltageSource](a)
    sensor = Modules.Cast[IVoltageSource](a)

    print("DC Voltage: ", sensor.GetDCVoltage())
    source.SetDCVoltage(2.0)
    print("DC Voltage: ", sensor.GetDCVoltage())

# See PyCharm help at https://www.jetbrains.com/help/pycharm/
