
var path = require("path");

module.exports = {
    entry: "./src/App.fsproj",
    outDir: "dist",
    babel: {
        presets: [["./node_modules/@babel/preset-env", { modules: "commonjs" }]],
        sourceMaps: false,
    },
    // The `onCompiled` hook (optional) is raised after each compilation
    onCompiled() {
        console.log("Compilation finished!")
    }
}