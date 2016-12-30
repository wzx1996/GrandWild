### Shader Importer

This is a utility package for maintaining a list of SPIR-V shaders.

It reads from a JSON file to decide what files to read. Supports both GLSL and SPIR-V files.

You may either use the whole package to maintain a shader list, or just use
org.flamerat.GrandWild.ShaderImporter.Glsl2Spv(), which wraps a glSlang compiler,
to compile GLSL shaders into SPIR-V shaders.