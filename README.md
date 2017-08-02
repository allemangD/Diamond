# Diamond

This project wraps OpenGL objects with managed .NET types. It uses OpenTK for most of the GL function bindings, and relies on OpenTK's math libraries. This is intended mainly as a convenience for writing simple OpenGL4 programs in .NET.

The wrapper classes use reflection to extract some information about .NET types, allowing them to be seamlessly packed into OpenGL buffers, making it very easy to manipulate buffers with typical .NET collections.

The attributes `[VertexData]` and `[VertexPointer]` indicate which members of a struct should be sent to which index of a shader. The built-in .NET Layout attributes should also be respected.
