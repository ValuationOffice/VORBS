module.exports = function () {

    var client = './ClientApp';

    var config = {
        /**
        * Files paths
        * Will change the file paths in the future
        * when the Scripts directory is cleansed
        * of any redundant files.
        */
        alljs: [
            './ClientApp/Administration/*.js',
            './ClientApp/Help Page/*.js',
            './ClientApp/MyBookings/*.js',
            './ClientApp/NewBooking/*.js',
            './ClientApp/Services/*.js',
            './ClientApp/Shared Functions/*.js'
        ],
        clientModules: [
            './ClientApp/app.module.js',
            './ClientApp/Administration/*.module.js',
            './ClientApp/Help Page/*.module.js',
            './ClientApp/MyBookings/*.module.js',
            './ClientApp/NewBooking/*.module.js',
            './ClientApp/Services/*.module.js',
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