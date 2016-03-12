//define(['ui', 'scriptrunner'], function (ui, scriptrunner) {
    var ui = require('../ui.js');
    var scriptrunner = require('../scriptrunner.js');
    //return {
    module.exports = {
        parameters: [1],
        execute: function (ctx) {               
            scriptrunner.evaluateExpression(ctx.parameters[0], function (result) {
                ui.print(result);
                ctx.complete();
            });
        }
    };
//});