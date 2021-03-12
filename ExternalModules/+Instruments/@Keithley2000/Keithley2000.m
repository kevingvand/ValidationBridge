classdef Keithley2000
    
    properties
        Implements
    end
    
    methods
        function obj = Keithley2000()
            obj.Implements = ["IVoltageSensor", "test"];
        end
       
        function name = GetName(obj)
            name = "Keysight2000";
        end
        
        function description = GetDescription(obj)
            description = "Keysight2000 multi meter communication module";
        end
        
        function connected = Connect(obj, connectionString)
            % Implementation of the Connect method %
        end
        
        function acVoltage = GetACVoltage(obj)
            % Implementation of the GetACVoltage method %
        end
        
        function dcVoltage = GetDCVoltage(obj)
            % Implementation of the GetDCVoltage method %
        end
    end
    
end

