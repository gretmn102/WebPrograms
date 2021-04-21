// Note this only includes basic configuration for development mode.
// For a more comprehensive configuration check:
// https://github.com/fable-compiler/webpack-config-template

const path = require("path")
const DotenvPlugin = require("dotenv-webpack")
var webpack = require("webpack");

module.exports = (env, argv) => {
    // extract build mode from command-line
    var mode = argv.mode;
    if (mode === undefined) {
        mode = "development";
    }
    console.log(mode);

    return {
        mode: mode,
        entry: "./src/App.fsproj",
        output: {
            path: path.join(__dirname, "./public"),
            filename: "bundle.js"
        },
        plugins: mode === "development" ?
            // development mode plugins
            [
                new DotenvPlugin({
                    path: path.join(__dirname, ".env"),
                    silent: true,
                    systemvars: true
                }),

                new webpack.HotModuleReplacementPlugin()
            ]
            :
            // production mode plugins
            [
                new DotenvPlugin({
                    path: path.join(__dirname, ".env"),
                    silent: true,
                    systemvars: true
                })
            ],
        devServer: {
            contentBase: "./public",
            port: 8080,
            hot: true,
            inline: true
        },
        module: {
            rules: [{
                test: /\.fs(x|proj)?$/,
                use: {
                    loader: "fable-loader",
                    options: {
                        define: mode === "development" ? ["DEVELOPMENT"] : []
                    }
                }
            }
        ]
        }
    }
}