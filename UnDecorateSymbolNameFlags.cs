enum UnDecorateSymbolNameFlags
{
    Complete = 0x0000, // Enable full undecoration
    NoLeadingUnderscores = 0x0001, // Remove leading underscores from MS extended keywords
    NoMSKeywords = 0x0002, // Disable expansion of MS extended keywords
    NoFunctionReturns = 0x0004, // Disable expansion of return type for primary declaration
    NoAllocationModel = 0x0008, // Disable expansion of the declaration model
    NoAllocationLanguage = 0x0010, // Disable expansion of the declaration language specifier
    NoMSThisType = 0x0020, // NYI Disable expansion of MS keywords on the 'this' type for primary declaration
    NoCVThisType = 0x0040, // NYI Disable expansion of CV modifiers on the 'this' type for primary declaration
    NoThisType = 0x0060, // Disable all modifiers on the 'this' type
    NoAccessSpecifiers = 0x0080, // Disable expansion of access specifiers for members
    NoThrowSignatures = 0x0100, // Disable expansion of 'throw-signatures' for functions and pointers to functions
    NoMemberType = 0x0200, // Disable expansion of 'static' or 'virtual'ness of members
    NoReturnUDTModel = 0x0400, // Disable expansion of MS model for UDT returns
    Decode32Bit = 0x0800, // Undecorate 32-bit decorated names
    NameOnly = 0x1000, // Crack only the name for primary declaration;
                       //  return just [scope::]name.  Does expand template params
    NoArguments = 0x2000, // Don't undecorate arguments to function
    NoSpecialSyms = 0x4000 // Don't undecorate special names (v-table, vcall, vector xxx, metatype, etc)
}
