define(function () {
	var elements = {};
    var elementsOfType = {};
    // TODO: Functions are just another type of element?
	var functions = {};
	
	var getElement = function (elementName) {
		var element = elements[elementName];
		if (!element) {
			throw 'No element named ' + elementName;
		}
		return element;	
	};
	
	var set = function (element, attribute, value) {
		element.attributes[attribute] = value;
	};
	
	var get = function (element, attribute) {
		var result = element.attributes[attribute];
		if (typeof result === 'undefined') {
            for (var idx in element.inheritedTypes) {
                var inheritedTypeElement = getElement(element.inheritedTypes[idx]);
                if (attributeExists(inheritedTypeElement, attribute)) {
                    result = get(inheritedTypeElement, attribute);
                    break;
                }
            }
            
            if (typeof result === 'undefined') {
                result = null;
            }
        }
        
        // TODO: Check for listextend
		return result;
	};
    
    var addInheritedType = function (element, typeName) {
        // TODO: Check for circular inheritance
        element.inheritedTypes.splice(0, 0, typeName);
    };
    
    var attributeExists = function (element, attribute) {
        if (attribute in element.attributes) return true;
        for (var idx in element.inheritedTypes) {
            var inheritedTypeElement = getElement(element.inheritedTypes[idx]);
            return attributeExists(inheritedTypeElement, attribute);
        }
        // TODO: Optional includeExtendableFields parameter to check for listexend,
        // as per WorldModel.Fields.Exists
        return false;
    };
	
	var isElement = function (elementName) {
		return elementName in elements;
	};
	
	var create = function (elementName, elementType, elementSubType) {
        var inheritedTypes = [];
        if (elementType == 'object') {
            if (!elementSubType) throw 'Object must have a subtype';
            inheritedTypes.push('default' + elementSubType);
        }
		var element = {
			name: elementName,
            elementType: elementType,
            attributes: {},
            inheritedTypes: inheritedTypes
		};
        elements[elementName] = element;
        if (!elementsOfType[elementType]) elementsOfType[elementType] = {};
        elementsOfType[elementType][elementName] = element;
        return element;
	};
	
	var addFunction = function (functionName, script, parameters) {
		functions[functionName] = {
			script: script,
			parameters: parameters
		};
	};
	
	var functionExists = function (functionName) {
		return functionName in functions;
	};
	
	var getFunction = function (functionName) {
		return functions[functionName].script;
	};
	
	var getFunctionDefinition = function (functionName) {
		return functions[functionName];
	};
	
	var dump = function () {
		console.log("Elements:");
		console.log(elements);
		console.log("Functions:");
		console.log(functions);
	};
	
	return {
		set: set,
		get: get,
        addInheritedType: addInheritedType,
		isElement: isElement,
        getElement: getElement,
		create: create,
		addFunction: addFunction,
		functionExists: functionExists,
		getFunction: getFunction,
		getFunctionDefinition: getFunctionDefinition,
		dump: dump
	};
});