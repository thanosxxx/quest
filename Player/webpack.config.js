var path = require('path');

module.exports = {
    watch: true,
    entry: {
        app: ['./quest.js']
    },
    path: path.resolve(__dirname, 'dist'),
    output: {
        filename: 'quest.js'
    }
}