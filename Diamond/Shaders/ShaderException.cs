using System;
using System.Runtime.Serialization;

namespace Diamond.Shaders
{
    /// <summary>
    /// Exception relating to <code>Shader</code> and <code>Program</code> operations.
    /// </summary>
    [Serializable]
    public class ShaderException : Exception
    {
        public ShaderException()
        {
        }

        public ShaderException(string message)
            : base(message)
        {
        }

        public ShaderException(string message, Exception inner)
            : base(message, inner)
        {
        }

        protected ShaderException(
            SerializationInfo info,
            StreamingContext context)
            : base(info, context)
        {
        }
    }
}