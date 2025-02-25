using System;
using System.Runtime.Serialization;

namespace PortalToUnity
{
    public class FigureRemovedException : Exception
    {
        public FigureRemovedException()
        {
        }

        public FigureRemovedException(string message) : base(message)
        {
        }

        public FigureRemovedException(string message, Exception inner) : base(message, inner)
        {
        }

        protected FigureRemovedException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }

    public class FigureErrorException : Exception
    {
        public FigureErrorException()
        {
        }

        public FigureErrorException(string message) : base(message)
        {
        }

        public FigureErrorException(string message, Exception inner) : base(message, inner)
        {
        }

        protected FigureErrorException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}