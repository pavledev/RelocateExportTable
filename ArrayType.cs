using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RelocateExportTable
{
    class ArrayType
    {
        public uint Count
        {
            get;
            set;
        }

        public ulong Length
        {
            get;
            set;
        }

        public bool IsConst
        {
            get;
            set;
        }

        public bool IsVolatile
        {
            get;
            set;
        }
    }
}
