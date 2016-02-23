define(function () {
	var elements = {};
	var functions = {};
	
	var getElement = function (elementName) {
		var element = elements[elementName];
		if (!element) {
			throw 'No element named ' + elementName;
		}
		return element;	
	};
	
	var set = function (elementName, attribute, value) {
		var element = getElement(elementName);
		element.attributes[attribute] = value;
	};
	
	var get = function (elementName, attribute) {
		var element = getElement(elementName);
		var result = element.attributes[attribute];
		if (typeof result === 'undefined') {
            // TODO: Check the attributes of inherited types
            result = null;
        }
		return result;
	};
	
	var isElement = function (elementName) {
		return elementName in elements;
	};
	
	var create = function (elementName, elementType) {
		elements[elementName] = {
			name: elementName,
            elementType: elementType,
            attributes: {},
            types: []
		};
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
		isElement: isElement,
		create: create,
		addFunction: addFunction,
		functionExists: functionExists,
		getFunction: getFunction,
		getFunctionDefinition: getFunctionDefinition,
		dump: dump
	};
});