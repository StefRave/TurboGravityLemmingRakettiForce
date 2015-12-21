using System;
using System.Diagnostics;
using System.Text;

namespace TurboPort.Event
{
    /// <summary>
    /// Information needed for serialization
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public class GameEventAttribute : Attribute
    {
        public string FourLetterIdentifier { get; }

        public int IdFromFourLetters
        {
            get
            {
                int idFromFourLeters = 0;
                foreach (char c in FourLetterIdentifier)
                    idFromFourLeters = (idFromFourLeters << 6) + (c & 0x3f);
                return idFromFourLeters;
            }
        }

        public GameEventAttribute(string fourLetterIdentifier)
        {
            this.FourLetterIdentifier = fourLetterIdentifier;
            Debug.Assert(Encoding.ASCII.GetByteCount(fourLetterIdentifier) == 4, "Four letter identifier must be 4 characters/bytes");
        }
    }
}