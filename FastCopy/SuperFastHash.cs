using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FastCopy
{
    class SuperFastHash
    {
        public unsafe UInt32 Hash(Byte[] dataToHash)
        {
            Int32 dataLength = dataToHash.Length;
            if (dataLength == 0)
                return 0;
            UInt32 hash = (UInt32)dataLength;
            Int32 remainingBytes = dataLength & 3; // mod 4
            Int32 numberOfLoops = dataLength >> 2; // div 4

            fixed (byte* firstByte = &(dataToHash[0]))
            {
                /* Main loop */
                UInt16* data = (UInt16*)firstByte;
                for (; numberOfLoops > 0; numberOfLoops--)
                {
                    hash += *data;
                    UInt32 tmp = (UInt32)(*(data + 1) << 11) ^ hash;
                    hash = (hash << 16) ^ tmp;
                    data += 2;
                    hash += hash >> 11;
                }
                switch (remainingBytes)
                {
                    case 3:
                        hash += *data;
                        hash ^= hash << 16;
                        hash ^= ((UInt32)(*(((Byte*)(data)) + 2))) << 18;
                        hash += hash >> 11;
                        break;
                    case 2:
                        hash += *data;
                        hash ^= hash << 11;
                        hash += hash >> 17;
                        break;
                    case 1:
                        hash += *((Byte*)data);
                        hash ^= hash << 10;
                        hash += hash >> 1;
                        break;
                    default:
                        break;
                }
            }

            /* Force "avalanching" of final 127 bits */
            hash ^= hash << 3;
            hash += hash >> 5;
            hash ^= hash << 4;
            hash += hash >> 17;
            hash ^= hash << 25;
            hash += hash >> 6;

            return hash;
        }
    }
}
