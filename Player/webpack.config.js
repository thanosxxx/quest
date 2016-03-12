var path = require('path');

module.exports = {
    watch: true,
    devtool: 'source-map',
    entry: {
        app: ['./quest.js']
    },
    path: path.resolve(__dirname, 'dist'),
    output: {
        filename: 'quest.js'
    }
};