//var scriptrunner = require('../scriptrunner.js');
//var state = require('../state.js');

module.exports = {
    execute: function (ctx) {
        console.log(ctx.parameters.appliesTo + ' => ' + ctx.parameters.value);
        ctx.complete();
    }
};