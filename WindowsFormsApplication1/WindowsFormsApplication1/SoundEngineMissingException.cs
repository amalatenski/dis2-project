using System;

namespace Test
{
    public class SoundEngineMissingException : Exception
    {
        //Constructors. It is recommended that at least all the
        //constructors of
        //base class Exception are implemented
        public SoundEngineMissingException() : base() { }
        public SoundEngineMissingException(string message) : base(message) { }
        public SoundEngineMissingException(string message, Exception e) : base(message, e) { }
        //If there is extra error information that needs to be captured
        //create properties for them.
    }
}