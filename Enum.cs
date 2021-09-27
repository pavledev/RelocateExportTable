using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RelocateExportTable
{
    class Enum
    {
        public uint Id
        {
            get;
            set;
        }

        public uint BaseType
        {
            get;
            set;
        }

        public ulong Length
        {
            get;
            set;
        }

        public string Name
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
