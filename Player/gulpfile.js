var gulp = require('gulp');  
var sourcemaps = require('gulp-sourcemaps');  
var ts = require('gulp-typescript');  
var babel = require('gulp-babel');
var uglify = require('gulp-uglify');
var tsProject = ts.createProject('./tsconfig.json');

gulp.task('asl4', function () {
    gulp.src('./node_modules/gulp-babel/node_modules/babel-core/browser-polyfill.min.js')
        .pipe(gulp.dest('.'));
      
    return gulp.src('asl4.ts')
        .pipe(sourcemaps.init())
        .pipe(ts(tsProject))
        .pipe(babel())
        //.pipe(uglify())
        .pipe(sourcemaps.write('.'))
        .pipe(gulp.dest('.'));
});

gulp.task('lib', function () {
    return gulp.src('lib/**/*')
        .pipe(gulp.dest('dist/lib'));
});

gulp.task('scripts', function () {
    return gulp.src('scripts/**/*')
        .pipe(gulp.dest('dist/scripts'));
});

gulp.task('ui', function () {
    return gulp.src('ui/**/*')
        .pipe(gulp.dest('dist/ui'));
});

gulp.task('files', function () {
    return gulp.src([
        '*.html',
        '*.asl',
        '*.aslx',
        '*.cas',
        '*.js'
    ]).pipe(gulp.dest('dist'));
});

gulp.task('default', ['lib', 'scripts', 'ui', 'files']);