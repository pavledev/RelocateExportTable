using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RelocateExportTable
{
    public class Function
    {
        public string MangledName
        {
            get;
            set;
        }

        public string DemangledName
        {
            get;
            set;
        }

        public string FullDemangledName
        {
            get;
            set;
        }

        public string ParentClassName
        {
            get;
            set;
        }

        public Access Access
        {
            get;
            set;
        }

        public uint RelativeVirtualAddress
        {
            get;
            set;
        }

        public bool IsStatic
        {
            get;
            set;
        }

        public bool IsVirtual
        {
            get;
            set;
        }

        public bool IsPure
        {
            get;
            set;
        }

        public string ReturnType
        {
            get;
            set;
        }

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

        public bool IsConstructor
        {
            get;
            set;
        }

        public bool IsDestructor
        {
            get;
            set;
        }

        public bool IsVariadic
        {
            get;
            set;
        }

        public List<string> Parameters
        {
            get;
            set;
        } = new List<string>();

        public bool ReturnsFunctionPointer
        {
            get;
            set;
        }
    }
}
