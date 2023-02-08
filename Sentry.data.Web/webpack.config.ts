import * as path from 'path';
import * as webpack from 'webpack';
import * as MiniCssExtractPlugin from 'mini-css-extract-plugin';
import * as CssMinimizerPlugin from 'css-minimizer-webpack-plugin';
import * as CopyWebpackPlugin from 'copy-webpack-plugin';

function config(env, argv): webpack.Configuration {
    let IS_PROD = argv.mode == 'production';
    return {
        entry: {
            // one entry per "bundle" goes in here
            'main': './src/main.ts',
            'main_css': './src/main.scss',
            'knockout': './src/knockout.ts',
            'site': { import: '!import-glob!./src/site.ts', dependOn: 'main' },
            'site_css': './src/site.scss',
            'datatables': { import: '/src/datatables.ts', dependOn: 'main' },
            'datatables_css': './src/datatables.scss',
            'prettycron': './src/prettycron.ts',
            'quill': './src/quill.ts',
            'quill_css': './src/quill.scss',
            'fancybox': './src/fancybox.ts',
            'fancybox_css': './src/fancybox.scss',
        },
        output: {
            clean: true,
            filename: (IS_PROD ? '[name].[contenthash].js' : '[name].local.js'),
            path: path.resolve(__dirname, 'dist')
        },
        devtool: 'source-map',   // enables source-maps
        plugins: [
            new MiniCssExtractPlugin({
                filename: (IS_PROD ? '[name].[contenthash].css' : '[name].local.css')
            }),
            // solution for npm build failing for prettycron due to issue with later.js found here: https://github.com/bunkat/later/issues/155
            new webpack.EnvironmentPlugin({
                LATER_COV: false
            }),
            // The CopyWebpackPlugin lets you copy any files from sources to the dist folder.
            new CopyWebpackPlugin({
                patterns: [
                    { from: 'node_modules/@sentry-insurance/InternalFrontendTemplate/Images', to: 'images' }
                ]
            })
        ],
        optimization: {
            minimizer: [
                `...`,  // includes existing minimizers (specifically, terser-webpack-plugin for minimizing JS)
                new CssMinimizerPlugin(),  // adds support for mimizing CSS
            ]
        },
        module: {
            rules: [
                {  // support for *.css files
                    test: /\.css$/i,
                    use: [MiniCssExtractPlugin.loader, 'css-loader']
                },
                {  // support for *.scss and *.sass files
                    test: /\.s[ac]ss$/i,
                    use: [
                        MiniCssExtractPlugin.loader,
                        "css-loader",
                        // url rewriting for sass file imports
                        // https://webpack.js.org/loaders/sass-loader/#problems-with-url
                        "resolve-url-loader",
                        'sass-loader',
                    ],
                },
                {   // support for *.ts files
                    test: /\.ts$/,
                    use: ['ts-loader'],
                    exclude: /node_modules/,
                },
                {
                    // expose dateSelect function in yadcf
                    test: /jquery.dataTables.yadcf\.js$/,
                    loader: 'string-replace-loader',
                    options: {
                        search: 'dateSelectSingle: dateSelectSingle,',
                        replace: 'dateSelectSingle: dateSelectSingle, dateSelect: dateSelect,',
                    }
                }
            ]
        },
        resolve: {
            extensions: ['.ts', '.js'],  // this allows us to use "import 'abc'" syntax, and either abc.ts or abc.js is found
            alias: {
                // this maps paths found in sentry-styles.css to the correct locations
                // Without these, you'll get errors such as
                //    Module not found: Error: Can't resolve '../img/overlays/01.png'
                //    Module not found: Error: Can't resolve '/fonts/SourceCodePro/SourceSerifPro-Regular.woff' 
                '../img': '@sentry-insurance/mdbootstrap/img',
                '/fonts': '../fonts',
            }
        }
    }

}

export default config;