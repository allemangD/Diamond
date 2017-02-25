using System;
using System.Runtime.Serialization;

namespace hexworld.Util
{
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