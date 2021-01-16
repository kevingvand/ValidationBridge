classdef KeysightB2901A
    %KEYSIGHTB2901A Summary of this class goes here
    %   Detailed explanation goes here
    
    properties
        Implements
        ACVoltage
        DCVoltage
    end
    
    methods
        function obj = KeysightB2901A()
            obj.Implements = ["IVoltageSource", "IVoltageSensor"];
            obj.ACVoltage = 120;
            obj.DCVoltage = 5;
        end
        
        function name = GetName(obj)
            name = "KeysightB2901A2";
        end
        
        function description = GetDescription(obj)
            description = "KeysightB2901A2 source meter communication module";
        end
        
        function connected = Connect(obj, connectionString)
            connected = true;
        end
        
        function connected = ConnectGPIB(obj, gpibPort, gpibIndex)
            connected = false;
        end
        
        function acVoltage = GetACVoltage(obj)
            acVoltage = obj.ACVoltage;
        end
        
        function dcVoltage = GetDCVoltage(obj)
            dcVoltage = obj.DCVoltage;
        end
        
        function obj = SetACVoltage(obj, acVoltage)
            obj.ACVoltage = acVoltage;
        end
        
        function obj = SetDCVoltage(obj, dcVoltage)
            obj.DCVoltage = dcVoltage;
        end
    end
end

