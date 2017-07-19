/*
This file is the main entry point for defining Gulp tasks and using Gulp plugins.
Click here to learn more. https://go.microsoft.com/fwlink/?LinkId=518007
*/

var gulp = require('gulp');
var args = require('yargs').argv;
var $ = require('gulp-load-plugins')({ lazy: true });
var config = require('./gulp.config')();

gulp.task('help', $.taskListing);
gulp.task('default', ['help']);

gulp.task('vet', function () {
    log('Analyzing source with ESLint');

    return gulp
        .src(config.alljs)
        .pipe($.if(args.verbose, $.print())) // if --verbose option was added to task
        .pipe($.eslint())
        .pipe($.eslint.format())
        .pipe($.eslint.failAfterError())
});

gulp.task('test', ['vet'], function (done) {
    startTests(true /* singleRun */, done);
});

gulp.task('autotest', ['vet'], function (done) {
    startTests(false /* singleRun */, done);
});

///////////////////////////////////////////////////

function startTests(singleRun, done) {
    var karma = require('karma').Server;
    var excludeFiles = [];

    karma.start({
        configFile: __dirname + '/karma.conf.js',
        exclude: excludeFiles,
        singleRun: !!singleRun
    }, done());

}

function log(msg) {
    if (typeof (msg) === 'object') {
        for (var item in msg) {
            if (msg.hasOwnProperty(item)) {
                $.util.log($.util.colors.blue(msg[item]));
            }
        }
    } else {
        $.util.log($.util.colors.blue(msg));
    }
}
