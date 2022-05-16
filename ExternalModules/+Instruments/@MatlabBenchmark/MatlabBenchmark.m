classdef MatlabBenchmark

    properties
        Implements
    end
    
    methods
        function obj = MatlabBenchmark()
            obj.Implements = ["IBenchmark"];
        end
        
        function name = GetName(obj)
            name = "MatlabBenchmark";
        end
        
        function description = GetDescription(obj)
            description = "Module for testing the performance of MATLAB modules";
        end
        
        function result = Add(obj, a, b)
            result = a + b;
        end
        
        function result = DelayedAdd(obj, a, b)
            pause(10/1000);
            result = obj.Add(a, b);
        end
        
        function result = AddToArray(obj, array, value)
            result = array + value;
        end
        
        function result = DelayedAddToArray(obj, array, value)
            pause(10/1000);
            result = obj.AddToArray(array, value);
        end
    end
end

