define(['state'], function (state) {
    var asyncFunctions = {
        'GetInput': function (args, complete) {
            // TODO: Override input handler
            
            setTimeout(function () {
                complete("test");
            }, 200);
        }
    };
    
    var functions = {
        // String Functions
        'Left': function (args) {
            var input = args[0];
            var length = args[1];
            if (input == null) return '';
            return input.substring(0, length);
        },
        'Right': function (args) {
            var input = args[0];
            var length = args[1];
            if (input == null) return '';
            return input.substring(input.length - length - 1);
        },
        'Mid': function (args) {
            var input = args[0];
            var start = args[1];
            if (args.length > 2) {
                var length = args[2];
                return input.substr(start - 1, length);
            }
            return input.substr(start - 1);
        },
        'UCase': function (args) {
            var input = args[0];
            return input.toUpperCase();
        },
        'LCase': function (args) {
            var input = args[0];
            return input.toLowerCase();
        },
        'LengthOf': function (args) {
            var input = args[0];
            if (typeof input === 'undefined' || input === null) return 0;
            return input.length;
        },
        'CapFirst': function (args) {
            var input = args[0];
            return input.substring(0, 1).toUpperCase() + input.substring(1);
        },
        'Instr': function (args) {
            var input, search;
            if (args.length > 2) {
                var start = args[0];
                input = args[1];
                search = args[2];
                return input.indexOf(search, start - 1) + 1;
            }
            input = args[0];
            search = args[1];
            return input.indexOf(search) + 1;
        },
        'InstrRev': function (args) {
            var input, search;
            if (args.length > 2) {
                var start = args[0];
                input = args[1];
                search = args[2];
                return input.lastIndexOf(search, start - 1) + 1;
            }
            input = args[0];
            search = args[1];
            return input.lastIndexOf(search) + 1;
        },
        'StartsWith': function (args) {
            var input = args[0];
            var search = args[1];
            return input.indexOf(search) === 0;
        },
        'EndsWith': function (args) {
            var input = args[0];
            var search = args[1];
            return input.indexOf(search) === input.length - search.length;
        },
        'Split': function (args) {
            var input = args[0];
            var splitChar = args[1];
            return input.split(splitChar);
        },
        'Join': function (args) {
            var input = args[0];
            var joinChar = args[1];
            // TODO: Handle other types
            if (!input.type || input.type != 'stringlist') {
                throw "Unhandled type passed to Join";
            }
            return input.value.join(joinChar);
        },
        'IsNumeric': function (args) {
            var input = args[0];
            return !isNaN(parseFloat(input)) && isFinite(input);
        },
        'Replace': function (args) {
            var input = args[0];
            var oldString = args[1];
            var newString = args[2];
            return input.split(oldString).join(newString);
        },
        'Trim': function (args) {
            var input = args[0];
            return input.trim();
        },
        'LTrim': function (args) {
            var input = args[0];
            return input.replace(/^\s+/,"");
        },
        'RTrim': function (args) {
            var input = args[0];
            return input.replace(/\s+$/,"");
        },
        'Asc': function (args) {
            var input = args[0];
            return input.charCodeAt(0);
        },
        'Chr': function (args) {
            var input = args[0];
            return String.fromCharCode(input);
        },
        // ExpressionOwner functions
        'HasString': function (args) {
            var element = args[0];
            var attribute = args[1];
            return state.hasAttributeOfType(element, attribute, 'string');
        },
        'GetString': function (args) {
            var element = args[0];
            var attribute = args[1];
            return state.getAttributeOfType(element, attribute, 'string');
        },
        'HasBoolean': function (args) {
            var element = args[0];
            var attribute = args[1];
            return state.hasAttributeOfType(element, attribute, 'boolean');
        },
        'GetBoolean': function (args) {
            var element = args[0];
            var attribute = args[1];
            return state.getAttributeOfType(element, attribute, 'boolean');
        },
        'HasInt': function (args) {
            var element = args[0];
            var attribute = args[1];
            return state.hasAttributeOfType(element, attribute, 'int');
        },
        'GetInt': function (args) {
            var element = args[0];
            var attribute = args[1];
            return state.getAttributeOfType(element, attribute, 'int');
        },
        'HasDouble': function (args) {
            var element = args[0];
            var attribute = args[1];
            return state.hasAttributeOfType(element, attribute, 'double');
        },
        'GetDouble': function (args) {
            var element = args[0];
            var attribute = args[1];
            return state.getAttributeOfType(element, attribute, 'double');
        },
        'HasScript': function (args) {
            var element = args[0];
            var attribute = args[1];
            return state.hasAttributeOfType(element, attribute, 'script');
        },
        'HasObject': function (args) {
            var element = args[0];
            var attribute = args[1];
            return state.hasAttributeOfType(element, attribute, 'object');
        },
        'GetObject': function (args) {
            var name = args[0];
            return state.tryGetElement(name);
        },
        'HasAttribute': function (args) {
            var element = args[0];
            var attribute = args[1];
            return state.hasAttribute(element, attribute);
        },
        'GetAttributeNames': function (args) {
            var element = getParameter(args[0], 'GetAttributeNames', 'element');
            var includeInheritedAttributes = getParameter(args[1], 'GetAttributeNames', 'boolean');
            var result = state.newAttribute('stringlist');
            result.value = state.attributeNames(element, includeInheritedAttributes);
            return result;
        },
        'Contains': function (args) {
            var parent = args[0];
            var element = args[1];
            return state.contains(parent, element);
        },
        'NewStringList': function () {
            return state.newAttribute('stringlist');
        },
        'NewObjectList': function () {
            return state.newAttribute('objectlist');
        },
        'NewList': function () {
            return state.newAttribute('list');
        },
        'NewStringDictionary': function () {
            return state.newAttribute('stringdictionary');
        },
        'NewObjectDictionary': function () {
            return state.newAttribute('objectdictionary');
        },
        'NewScriptDictionary': function () {
            return state.newAttribute('scriptdictionary');
        },
        'NewDictionary': function () {
            return state.newAttribute('dictionary');
        },
        'ListContains': function (args) {
            var list = args[0];
            var item = args[1];
            checkIsList(list);
            return list.value.indexOf(item) !== -1;
        },
        'AllObjects': function () {
            var objects = state.getElements('object', 'object');
            var result = state.newAttribute('objectlist');
            result.value = objects;
            return result;
        },
        'AllExits': function () {
            var exits = state.getElements('object', 'exit');
            var result = state.newAttribute('objectlist');
            result.value = exits;
            return result;
        },
        'AllTurnScripts': function () {
            var turnScripts = state.getElements('object', 'turnscript');
            var result = state.newAttribute('objectlist');
            result.value = turnScripts;
            return result;
        },
        'AllCommands': function () {
            var commands = state.getElements('object', 'command');
            var result = state.newAttribute('objectlist');
            result.value = commands;
            return result;
        },
        'ListCount': function (args) {
            var list = args[0];
            checkIsList(list);
            return list.value.length;
        },
        'ListCombine': function (args) {
            var list1 = args[0];
            var list2 = args[1];
            checkIsList(list1);
            if (list1.type != list2.type) {
                throw 'Mismatched list types passed to ListCombine';
            }
            var result = state.newAttribute(list1.type);
            result.value = list1.value.concat(list2.value);
            return result;
        },
        'GetAllChildObjects': function (args) {
            var element = args[0];
            var result = state.newAttribute('objectlist');
            result.value = state.getAllChildren(element, 'object', 'object');
            return result;
        },
        'SafeXML': function (args) {
            var input = getParameter(args[0], 'SafeXML', 'string');
            return input.replace(/&/g, '&amp;').replace(/\</g, '&lt;').replace(/\>/g, '&gt;');
        }
    };
    
    var checkIsList = function (list) {
        if (list.type != 'list' && list.type != 'stringlist' && list.type != 'objectlist') {
            throw 'Value is not a list type';
        }
    };
    
    var getParameter = function (parameter, caller, type) {
        var actualType = state.typeOf(parameter);
        if (actualType !== type) {
            throw caller + ' function expected ' + type + ' parameter but was passed ' + actualType;
        }
        return parameter;
    };
    
    return {
        asyncFunctions: asyncFunctions,
        functions: functions
    };
});