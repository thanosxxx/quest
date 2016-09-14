//var scriptrunner = require('../scriptrunner.js');
var scriptParser = require('../scriptparser.js');
var expressions = require('../expressions.js');

module.exports = {
    create: function (line) {
        var scripts = require('../scripts.js');
        var parameters = scriptParser.getParameterInternal(line, '(', ')');
        var cases = scriptParser.getScriptLine(parameters.after).line;

        var finished = false;
        var result = [];
        var defaultScript = null;
        cases = scriptParser.removeSurroundingBraces(cases);

        while (!finished)
        {
            var scriptLine = scriptParser.getScriptLine(cases);
            cases = scriptLine.line;
            var remainingCases = scriptLine.after;

            if (cases) cases = cases.trim();

            if (cases)
            {
                if (cases.indexOf('case') === 0)
                {
                    var caseParameter = scriptParser.getParameterInternal(cases, '(', ')');
                    var expr = caseParameter.parameter;
                    var afterExpr = caseParameter.after;
                    var caseScript = scriptParser.getScriptLine(afterExpr).line;
                    var script = scripts.parseScript(caseScript);

                    var matchList = scriptParser.splitParameters(expr);
                    result = result.concat(matchList.map(function (match) {
                        return {
                            expr: match,
                            script: script
                        };
                    }));
                }
                else if (cases.indexOf('default') === 0) {
                    defaultScript = scripts.parseScript(cases.substring(8).trim());
                }
                else {
                    throw 'Invalid inside switch block: "' + cases + '"';
                }
            }

            cases = remainingCases;
            if (!cases) finished = true;
        }

        return {
            expression: expressions.parseExpression(parameters.parameter),
            cases: result,
            defaultScript: defaultScript
        };
    },
    execute: function (ctx) {
        console.log(ctx);
        // TODO...
        ctx.complete();
    }
};