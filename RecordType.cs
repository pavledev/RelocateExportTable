using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RelocateExportTable
{
    class RecordType
    {
        public uint BaseType
        {
            get;
            set;
        }

        public string TypeName
        {
            get;
            set;
        }

        public string Type
        {
            get;
            set;
        }

        public string Name
        {
            get;
            set;
        }

        public ulong Size
        {
            get;
            set;
        }

        public int Offset
        {
            get;
            set;
        }

        public int BitOffset
        {
            get;
            set;
        }

        public int BitSize
        {
            get;
            set;
        }

        public bool IsPointer
        {
            get;
            set;
        }

        public bool IsReference
        {
            get;
            set;
        }

        public uint PointerLevel
        {
            get;
            set;
        }

        public uint ReferenceLevel
        {
            get;
            set;
        }

        public bool IsArray
        {
            get;
            set;
        }

        public List<uint> ArrayCount
        {
            get;
            set;
        } = new List<uint>();

        public string FunctionReturnType
        {
            get;
            set;
        }

        public bool FunctionType
        {
            get;
            set;
        }

        public bool NoType
        {
            get;
            set;
        }

        public bool IsVariadicFunction
        {
            get;
            set;
        }

        public uint ParametersCount
        {
            get;
            set;
        }

        public List<string> FunctionParameters
        {
            get;
            set;
        } = new List<string>();

        public CallingConvention CallingConvention
        {
            get;
            set;
        }

        public bool IsTypeConst
        {
            get;
            set;
        }

        public bool IsTypeVolatile
        {
            get;
            set;
        }

        public bool IsPointerConst
        {
            get;
            set;
        }

        public bool IsPointerVolatile
        {
            get;
            set;
        }

        public bool IsFunctionConst
        {
            get;
            set;
        }

        public bool IsFunctionVolatile
        {
            get;
            set;
        }

        public bool IsFunctionPointer
        {
            get;
            set;
        }

        public bool FunctionReturnsFunctionPointer
        {
            get;
            set;
        }
    }
}
