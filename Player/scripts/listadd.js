define(['scriptrunner', 'lists'], function (scriptrunner, lists) {
    return {
        parameters: [2],
        execute: function (ctx) {               
            scriptrunner.evaluateExpressions(ctx.parameters, function (result) {
                lists.listAdd(result[0], result[1]);
                ctx.complete();
            });
        }
    };
});