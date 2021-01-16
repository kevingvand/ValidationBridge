package = meta.package.fromName("Instruments");

moduleCandidates = {}; moduleDefinitions = string.empty;
%%
for classIndex = 1:length(package.ClassList)
    class = package.ClassList(classIndex);
    
    if any(strcmp({class.PropertyList.Name}, 'Implements'))
        moduleCandidates(end+1) = {class};
    end
end

%% Minimized For loop

for i = 1:length(package.ClassList); class = package.ClassList(i); if any(strcmp({class.PropertyList.Name}, 'Implements')); moduleCandidates(end+1) = {class}; end; end;

%%

for(moduleIndex = 1:size(moduleCandidates, 2))
    
    moduleName = moduleCandidates{moduleIndex}.Name;
    moduleDefinitions(1, moduleIndex) = moduleName;
    moduleInstance = feval(moduleName);
    for(interfaceIndex = 1:size(moduleInstance.Implements, 2))
        moduleDefinitions(interfaceIndex + 1, moduleIndex) = moduleInstance.Implements(interfaceIndex);
    end
end

%% Minimzed For loop

for(i = 1:size(moduleCandidates, 2)); n = moduleCandidates{i}.Name; moduleDefinitions(1, i) = n; moduleInstance = feval(n); for(j = 1:size(moduleInstance.Implements, 2)); moduleDefinitions(j + 1, i) = moduleInstance.Implements(j); end; end;

%%

test = {};
for(test(i) = 1:10)
   disp(test.i) 
end

%%
% At this point we have a list of all possible classes, we can define an
% instance using the names in the class list. Then, for each class we can
% check the interfaces they implement and go from there

% for each implementing interface we have to check if all properties and
% functions are available. There is one exception: Instance Id, this will
% dynamically be assigned by the Bridge.