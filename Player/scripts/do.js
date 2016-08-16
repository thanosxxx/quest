var scriptrunner = require('../scriptrunner.js');
var state = require('../state.js');

module.exports = {
    parameters: [2, 3],
    execute: function (ctx) {
        // TODO: Third parameter is a dictionary of parameters to pass in as locals

        scriptrunner.evaluateExpressions(ctx.parameters, function (result) {
            var script = state.getAttributeOfType(result[0], result[1], 'script');
            if (!script) {
                ctx.complete();
                return;
            }
            scriptrunner.getCallstack().push({
                script: script.script,
                locals: {},
                index: 0,
                onReturn: ctx.complete
            });
            scriptrunner.continueRunningScripts();
        });
    }
};