define(function () {
	var elements = {};
    var elementsOfType = {};
    var templateLookup = {};
    
    var newAttribute = function (type) {
        if (type == 'stringlist' || type == 'objectlist' || type == 'list') {
            return {
                value: [],
                type: type
            };
        }
        
        if (type == 'stringdictionary' || type == 'objectdictionary' || type == 'scriptdictionary' || type == 'dictionary') {
            return {
                value: {},
                type: type
            };
        }
        
        throw 'Unknown attribute type: ' + type;
    };
	
	var getElement = function (elementName) {
		var element = elements[elementName];
		if (!element) {
			throw 'No element named ' + elementName;
		}
		return element;	
	};
    
    var tryGetElement = function (elementName) {
        return elements[elementName];
    };
	
	var set = function (element, attribute, value) {
		element.attributes[attribute] = value;
	};
	
	var get = function (element, attribute) {
		var result = element.attributes[attribute];
		if (typeof result === 'undefined') {
            for (var idx in element.inheritedTypes) {
                var inheritedTypeElement = getElement(element.inheritedTypes[idx]);
                if (hasAttribute(inheritedTypeElement, attribute)) {
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
    
    var hasAttribute = function (element, attribute) {
        if (attribute in element.attributes) return true;
        for (var idx in element.inheritedTypes) {
            var inheritedTypeElement = getElement(element.inheritedTypes[idx]);
            return hasAttribute(inheritedTypeElement, attribute);
        }
        // TODO: Optional includeExtendableFields parameter to check for listexend,
        // as per WorldModel.Fields.Exists
        return false;
    };
    
    var hasAttributeOfType = function (element, attribute, type) {
        if (!hasAttribute(element, attribute)) return false;
        var value = get(element, attribute);
        return isValueOfType(value, type);
    };
    
    var getAttributeOfType = function (element, attribute, type) {
        if (!hasAttribute(element, attribute)) return defaultValue(type);
        var value = get(element, attribute);
        if (isValueOfType(value, type)) return value;
        return defaultValue(type);
    };
    
    var isValueOfType = function (value, type) {
        var actualType = typeOf(value);
        if (actualType == type) return true;
        if (actualType == 'int' && type == 'double') return true;
        return false;
    };
    
    var defaultValue = function (type) {
        if (type === 'boolean') return false;
        return null;
    };
    
    var typeOf = function (value) {
        if (typeof value === 'string') return 'string';
        if (typeof value === 'boolean') return 'boolean';
        if (typeof value === 'number') {
            if (value % 1 === 0) return 'int';
            return 'double';
        }
        if (value.type) return value.type;
        throw 'Unknown type';
    };
    
    var attributeNames = function (element, includeInheritedAttributes) {
        var result = Object.keys(element.attributes);
        if (includeInheritedAttributes) {
            for (var idx in element.inheritedTypes) {
                var inheritedTypeElement = getElement(element.inheritedTypes[idx]);
                var additionalAttributes = attributeNames(inheritedTypeElement);
                result = result.concat(additionalAttributes.filter(function (a) {
                    return result.indexOf(a) === -1;
                }));
            }
        }
        return result;
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
            type: 'element',
            elementType: elementType,
            elementSubType: elementSubType,
            attributes: {
                name: elementName,
                elementtype: elementType,
                type: elementSubType
            },
            inheritedTypes: inheritedTypes
		};
        elements[elementName] = element;
        if (!elementsOfType[elementType]) elementsOfType[elementType] = {};
        elementsOfType[elementType][elementName] = element;
        return element;
	};
    
    var addTemplate = function (element) {
        templateLookup[element.attributes.templatename] = element;
    }
    
    var getTemplate = function (name) {
        return templateLookup[name];
    }
    
    var getElements = function (elementType, elementSubType) {
        var elements = elementsOfType[elementType];
        var result = [];
        for (var key in elements) {
            var element = elements[key];
            if (!elementSubType || element.elementSubType == elementSubType) {
                result.push(element);
            }
        }
        return result;
    };
	
	var addFunction = function (functionName, script, parameters) {
        var fn = create(functionName, 'function');
        fn.attributes = {
			script: script,
			parameters: parameters
		};
	};
	
	var functionExists = function (functionName) {
		return functionName in elementsOfType['function'];
	};
	
	var getFunction = function (functionName) {
		return elementsOfType['function'][functionName].attributes.script;
	};
	
	var getFunctionDefinition = function (functionName) {
		return elementsOfType['function'][functionName].attributes;
	};
    
    var getDirectChildren = function (parent, elementType, elementSubType) {
        var allElements = getElements(elementType, elementSubType);
        return allElements.filter(function (element) {
            return element.attributes.parent == parent;
        });
    };
    
    var getAllChildren = function (parent, elementType, elementSubType) {
        var directChildren = getDirectChildren(parent, elementType, elementSubType);
        var result = [];
        for (var idx in directChildren) {
            var child = directChildren[idx];
            result = result.concat(child, getAllChildren(child, elementType, elementSubType));
        }
        return result;
    };
    
    var contains = function (parent, element) {
        if (!element.attributes.parent) return false;
        if (element.attributes.parent == parent) return true;
        return contains(parent, element.attributes.parent);
    };
    
    var nextUniqueId = {};
    
    var getUniqueId = function (prefix) {
        prefix = prefix || 'k';
        if (!(prefix in nextUniqueId)) {
            nextUniqueId[prefix] = 1;
        }
        var newId;
        do {
            newId = prefix + nextUniqueId[prefix]++;
        } while (isElement(newId));
        return newId;
    };
	
	var dump = function () {
		console.log("Elements:");
		console.log(elements);
	};
	
	return {
		newAttribute: newAttribute,
        set: set,
		get: get,
        addInheritedType: addInheritedType,
        hasAttribute: hasAttribute,
        hasAttributeOfType: hasAttributeOfType,
        getAttributeOfType: getAttributeOfType,
        typeOf: typeOf,
        attributeNames: attributeNames,
		isElement: isElement,
        getElement: getElement,
        tryGetElement: tryGetElement,
		create: create,
        addTemplate: addTemplate,
        getTemplate: getTemplate,
        getElements: getElements,
		addFunction: addFunction,
		functionExists: functionExists,
		getFunction: getFunction,
		getFunctionDefinition: getFunctionDefinition,
        getDirectChildren: getDirectChildren,
        getAllChildren: getAllChildren,
        contains: contains,
        getUniqueId: getUniqueId,
		dump: dump
	};
});