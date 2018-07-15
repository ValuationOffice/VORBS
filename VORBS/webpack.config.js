const path = require("path");
const CleanWebpackPlugin = require("clean-webpack-plugin");
const webpack = require("webpack");

module.exports = function (env, argv) {

    const isDevelopment = argv.mode === 'development';

    const config = {

        entry: {
            vendor: path.resolve(__dirname, "ClientApp", "vendor.js")
        },

        output: {
            path: path.resolve(__dirname, "build"),
            filename: "[name].js"
        },

        module: {
            rules: [{
                test: /\.(png|woff|woff2|eot|ttf|svg)$/,
                use: [{
                    loader: 'file-loader',
                    options: {
                        name: "fonts/[name].[ext]",
                        limit: 10000
                    }
                }]
            }, {
                test: /\.scss$/,
                use: [
                    "style-loader",
                    {
                        loader: "css-loader", options: {
                            sourceMap: true
                        }
                    }, {
                        loader: "sass-loader", options: {
                            sourceMap: true,
                            outputStyle: isDevelopment ? "nested" : "compressed"
                        }
                    }]
            },
            {
                test: /\.css$/,
                use: [
                    'style-loader',
                    'css-loader'
                ]
                }, {
                    test: /\.js$/,
                    use: [
                        {
                            loader: 'babel-loader',
                            options: {
                                babelrc: true
                            }
                        }
                    ]
                }]
        },

        plugins: [
            new CleanWebpackPlugin("./build/*"),
            new webpack.ProvidePlugin({
                $: "jquery",
                jQuery: "jquery",
                "window.jQuery": "jquery"
            })
        ],

        resolve: {
            extensions: [".js", ".jsx", ".ts", ".tsx", ".json"]
        }
    }

    return config;
}