classdef Keysight2001
    %KEYSIGHTB2901A Summary of this class goes here
    %   Detailed explanation goes here
    
    properties
        Implements
        ACVoltage
        DCVoltage
    end
    
    methods
        function obj = Keysight2001()
            obj.Implements = ["IVoltageSensor"];
            obj.ACVoltage = 120;
            obj.DCVoltage = 5;
        end
        
        function name = GetName(obj)
            name = "Keysight2001";
        end
        
        function description = GetDescription(obj)
            description = "KeysightB2901A2 multi meter communication module";
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
    end
end

