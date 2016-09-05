var scriptrunner = require('../scriptrunner.js');
var ui = require('../ui.js');

module.exports = {
    parameters: [2],
    execute: function (ctx) {               
        scriptrunner.evaluateExpression(ctx.parameters[1], function (data) {
            var request = ctx.parameters[0].expr;

            // Any changes here should also be reflected in CoreEditorScriptsOutput.aslx (validvalues for "request" command)
            // and also in the documentation https://github.com/textadventures/quest/blob/gh-pages/scripts/request.md

            switch (request) {
                case 'Quit':
                    console.log('Unhandled request type ' + request);
                    break;
                case 'UpdateLocation':
                    console.log('Unhandled request type ' + request);
                    break;
                case 'GameName':
                    console.log('Unhandled request type ' + request);
                    break;
                case 'FontName':
                    console.log('Unhandled request type ' + request);
                    break;
                case 'FontSize':
                    console.log('Unhandled request type ' + request);
                    break;
                case 'Background':
                    console.log('Unhandled request type ' + request);
                    break;
                case 'Foreground':
                    console.log('Unhandled request type ' + request);
                    break;
                case 'LinkForeground':
                    console.log('Unhandled request type ' + request);
                    break;
                case 'RunScript':
                    console.log('Unhandled request type ' + request);
                    break;
                case 'SetStatus':
                    console.log('Unhandled request type ' + request);
                    break;
                case 'ClearScreen':
                    console.log('Unhandled request type ' + request);
                    break;
                case 'PanesVisible':
                    console.log('Unhandled request type ' + request);
                    break;
                case 'ShowPicture':
                    console.log('Unhandled request type ' + request);
                    break;
                case 'Show':
                    ui.show(data);
                    break;
                case 'Hide':
                    ui.hide(data);
                    break;
                case 'SetCompassDirections':
                    console.log('Unhandled request type ' + request);
                    break;
                case 'Pause':
                    console.log('Unhandled request type ' + request);
                    break;
                case 'Wait':
                    console.log('Unhandled request type ' + request);
                    break;
                case 'SetInterfaceString':
                    console.log('Unhandled request type ' + request);
                    break;
                case 'RequestSave':
                    console.log('Unhandled request type ' + request);
                    break;
                case 'SetPanelContents':
                    console.log('Unhandled request type ' + request);
                    break;
                case 'Log':
                    console.log('Unhandled request type ' + request);
                    break;                
                case 'Speak':
                    console.log('Unhandled request type ' + request);
                    break;
                default:
                    throw 'Unhandled request type ';
            }
            
            ctx.complete();
        });
    }
};