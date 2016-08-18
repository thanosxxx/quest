var scriptrunner = require('../scriptrunner.js');
var state = require('../state.js');

module.exports = {
    parameters: [2, 3],
    execute: function (ctx) {
        scriptrunner.evaluateExpressions(ctx.parameters, function (result) {
            var script = state.getAttributeOfType(result[0], result[1], 'script');
            if (!script) {
                ctx.complete();
                return;
            }
            scriptrunner.getCallstack().push({
                script: script.script,
                locals: result[2] || {},
                index: 0,
                onReturn: ctx.complete
            });
            scriptrunner.continueRunningScripts();
        });
    }
};