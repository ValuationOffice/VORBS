module.exports = function () {

    var client = './Scripts';

    var config = {
        /**
        * Files paths
        * Will change the file paths in the future
        * when the Scripts directory is cleansed
        * of any redundant files.
        */
        alljs: [
            './Scripts/Administration/*.js',
            './Scripts/Help Page/*.js',
            './Scripts/MyBookings/*.js',
            './Scripts/NewBooking/*.js',
            './Scripts/Services/*.js',
            './Scripts/Shared Functions/*.js'
        ],
        clientModules: [
            './Scripts/app.module.js',
            './Scripts/Administration/*.module.js',
            './Scripts/Help Page/*.module.js',
            './Scripts/MyBookings/*.module.js',
            './Scripts/NewBooking/*.module.js',
            './Scripts/Services/*.module.js',
        ],
        npm: {
            json: require('./package.json'),
            directory: 'node_modules/'
        },
        client: client,

        // Karma Data Settings
        specHelpers: [
            client + '/Tests/Data/*.js'
        ]
    };

    config.karma = getKarmaOptions();

    return config;

    //////////////////////////////////////////////////

    // File Paths will be cleaned up once most of the 
    // redundant dependency files are sorted out.

    function getKarmaOptions() {
        var options = {
            files: [].concat(
                config.npm.directory + 'angular/angular.js',
                config.npm.directory + 'jquery/dist/jquery.js',
                config.npm.directory + 'angular-mocks/angular-mocks.js',
                config.npm.directory + 'angular-resource/angular-resource.js',
                config.npm.directory + 'moment/moment.js',
                config.specHelpers,
                config.clientModules,
                config.alljs,
                client + '/Tests/**/*.spec.js'
            ),
            coverage: {
                reporters: [{ type: 'text-summary' }]
            },
            exclude: [],
            preprocessors: {}
        };
        return options;
    }
}