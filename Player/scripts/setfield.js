define(['scriptrunner', 'state'], function (scriptrunner, state) {
    return {
        parameters: [3],
        execute: function (ctx) {               
            scriptrunner.evaluateExpressions(ctx.parameters, function (result) {
                state.set(result[0], result[1], result[2]);
                ctx.complete();
            });
        }
    };
});