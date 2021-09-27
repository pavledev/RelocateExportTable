public enum CallingConvention
{
    NearCdecl = 0x00, // near right to left push, caller pops stack
    FarCdecl = 0x01, // far right to left push, caller pops stack
    NearPascal = 0x02, // near left to right push, callee pops stack
    FarPascal = 0x03, // far left to right push, callee pops stack
    NearFast = 0x04, // near left to right push with regs, callee pops stack
    FarFast = 0x05, // far left to right push with regs, callee pops stack
    Skipped = 0x06, // skipped (unused) call index
    NearStdCall = 0x07, // near standard call
    FarStdCall = 0x08, // far standard call
    NearSysCall = 0x09, // near sys call
    FarSysCall = 0x0a, // far sys call
    ThisCall = 0x0b, // this call (this passed in register)
    MipsCall = 0x0c, // Mips call
    Generic = 0x0d, // Generic call sequence
    AlphaCall = 0x0e, // Alpha call
    PPCCall = 0x0f, // PPC call
    SHCall = 0x10, // Hitachi SuperH call
    ARMCall = 0x11, // ARM call
    AM33CALL = 0x12, // AM33 call
    TRICall = 0x13, // TriCore Call
    SH5Call = 0x14, // Hitachi SuperH-5 call
    M32RCall = 0x15, // M32R Call
    CLRCall = 0x16, // clr call
    Inline = 0x17, // Marker for routines always inlined and thus lacking a convention
    NearVector = 0x18, // near left to right push with regs, callee pops stack
    Swift = 0x19, // Swift calling convention
    Reserved = 0x20  // first unused call enumeration

    // Do NOT add any more machine specific conventions.  This is to be used for
    // calling conventions in the source only (e.g. __cdecl, __stdcall).
}