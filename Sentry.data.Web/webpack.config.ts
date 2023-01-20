import * as path from 'path';
import * as webpack from 'webpack';
import * as MiniCssExtractPlugin from 'mini-css-extract-plugin';
import * as CssMinimizerPlugin from 'css-minimizer-webpack-plugin';

function config(env, argv): webpack.Configuration {
    let IS_PROD = argv.mode == 'production';
    return {
        entry: {
            // one entry per "bundle" goes in here
            'common': './src/common.ts',
            'knockout': './src/knockout.ts',
            'site': '!import-glob!./src/site.ts'
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
            ]
        },
        resolve: {
            extensions: ['.ts', '.js'],  // this allows us to use "import 'abc'" syntax, and either abc.ts or abc.js is found
        }
    }

}

export default config;