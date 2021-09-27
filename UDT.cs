using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RelocateExportTable
{
    class UDT
    {
        public uint Id
        {
            get;
            set;
        }

        public string Name
        {
            get;
            set;
        }

        public string Type
        {
            get;
            set;
        }

        public ulong Length
        {
            get;
            set;
        }

        public UdtKind UdtKind
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
