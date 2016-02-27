define(['state', 'scripts'], function (state, scripts) {   
    var allowedVersions = [500, 510, 520, 530, 540, 550];
    
    var getXmlAttribute = function (node, attributeName) {
        var attribute = node.attributes[attributeName];
        if (!attribute) return null;
        return attribute.value;
    };
    
    var attributeLoaders = {
        'string': function (node, element, attributeName) {
            var attributeValue = node.textContent; 
            state.set(element, attributeName, attributeValue);
        },
        'stringlist': function (node, element, attributeName) {
            var list = [];
            for (var i = 0; i < node.childNodes.length; i++) {
                var childNode = node.childNodes[i];
                if (childNode.nodeName != 'value') continue;
                list.push(childNode.textContent);
            }
            state.set(element, attributeName, list);
        }
    };
    
    var loadElementAttributes = function (element, nodes) {
        for (var i = 0; i < nodes.length; i++) {
            var node = nodes[i];
            if (node.nodeType !== 1) continue;
            var attributeName = node.nodeName;
            if (attributeName == 'inherit') {
                var name = getXmlAttribute(node, 'name');
                state.addInheritedType(element, name);
            }
            else if (attributeName == 'object') {
                var child = elementLoaders.object(node);
                state.set(child, 'parent', element);
            }
            else {
                var attributeType = getXmlAttribute(node, 'type');
                // TODO: Default is boolean if element has no value
                if (!attributeType) attributeType = 'string';
                var loader = attributeLoaders[attributeType];
                if (loader) {
                    loader(node, element, attributeName);
                }
                else {
                    console.log('no attribute loader for type ' + attributeType);
                }
            }
        }
    };
    
    var elementLoaders = {
        'game': function (node) {
            var element = state.create('game', 'object', 'game');
            var name = getXmlAttribute(node, 'name');
            state.set(element, 'name', name);
            loadElementAttributes(element, node.childNodes);
        },
        'function': function (node) {
            var paramList;
            var parameters = getXmlAttribute(node, 'parameters');
            if (parameters) {
                paramList = parameters.split(/, ?/);
            }
            state.addFunction(getXmlAttribute(node, 'name'),
                scripts.parseScript(node.textContent),
                paramList);
        },
        'type': function (node) {
            var name = getXmlAttribute(node, 'name');
            var element = state.create(name, 'type');
            loadElementAttributes(element, node.childNodes);
        },
        'object': function (node) {
            var name = getXmlAttribute(node, 'name');
            var element = state.create(name, 'object', 'object');
            loadElementAttributes(element, node.childNodes);
            return element;
        }
    };
    
    var load = function (data) {
        var parser = new DOMParser();
        var doc = parser.parseFromString(data, 'application/xml');
        var firstNode = 0;
        for (var i = 0; i < doc.childNodes.length; i++) {
            if (doc.childNodes[i].nodeType === 1) {
                firstNode = i;
                break;
            }
        }
        var asl = doc.childNodes[firstNode];
        if (asl.nodeName !== 'asl') {
            throw 'File must begin with an ASL element';
        }
        var versionAttribute = asl.attributes['version'];
        if (!versionAttribute) {
            throw 'No ASL version number found';
        }
        var version = parseInt(versionAttribute.value);
        if (allowedVersions.indexOf(version) === -1) {
            throw 'Unrecognised ASL version number';
        }
        
        for (var i = 1; i < asl.childNodes.length; i++) {
            if (asl.childNodes[i].nodeType !== 1) continue;
            var loader = elementLoaders[asl.childNodes[i].nodeName];
            if (loader) {
                loader(asl.childNodes[i]);
            }
            else {
                console.log('no loader for ' + asl.childNodes[i].nodeName);
            }
        }
        
        state.dump();
    };
    
    return {
        load: load
    };
});