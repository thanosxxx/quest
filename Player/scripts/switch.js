var scriptrunner = require('../scriptrunner.js');
var scriptParser = require('../scriptparser.js');
var expressions = require('../expressions.js');

module.exports = {
    create: function (line) {
        var scripts = require('../scripts.js');
        var parameters = scriptParser.getParameterInternal(line, '(', ')');
        
        // TODO...

        return {
            expression: expressions.parseExpression(parameters.parameter)
        };
    },
    execute: function (ctx) {
        // TODO...
        ctx.complete();
    }
};