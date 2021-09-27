using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RelocateExportTable
{
    class FunctionType
    {
        public CallingConvention CallingConvention
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
