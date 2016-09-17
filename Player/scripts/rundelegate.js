var scriptrunner = require('../scriptrunner.js');
var state = require('../state.js');

module.exports = {
    minParameters: 2,
    execute: function (ctx) {
        scriptrunner.evaluateExpressions(ctx.parameters, function (result) {
            var element = result[0];
            var delName = result[1];
            var impl = state.getAttributeOfType(element, delName, 'delegateimplementation');
            if (!impl) {
                throw 'Object "' + element.name + '" has no delegate implementation "' + delName + '"';
            }

            var delegateDefinition = state.getElement(impl.delegateType);
            var script = impl.script;
            if (!script) {
                ctx.complete();
                return;
            }
            
            var locals = {};
            if (result.length > 2) {
                var paramNames = state.getAttributeOfType(delegateDefinition, 'paramnames', 'stringlist');
                for (var i = 0; i < paramNames.value.length; i++) {
                    locals[paramNames.value[i]] = result[2 + i];
                }
            }
            locals['this'] = element;
            
            scriptrunner.getCallstack().push({
                script: script,
                locals: locals,
                index: 0,
                onReturn: ctx.complete
            });
            scriptrunner.continueRunningScripts();
        });
    }
};