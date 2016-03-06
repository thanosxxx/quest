define(['scriptrunner', 'dictionaries'], function (scriptrunner, dictionaries) {
    return {
        parameters: [3],
        execute: function (ctx) {               
            scriptrunner.evaluateExpressions(ctx.parameters, function (result) {
                dictionaries.dictionaryAdd(result[0], result[1], result[2]);
                ctx.complete();
            });
        }
    };
});