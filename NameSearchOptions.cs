enum NameSearchOptions
{
    nsNone = 0,
    nsfCaseSensitive = 0x1,
    nsfCaseInsensitive = 0x2,
    nsfFNameExt = 0x4,
    nsfRegularExpression = 0x8,
    nsfUndecoratedName = 0x10,
    nsCaseSensitive = nsfCaseSensitive,
    nsCaseInsensitive = nsfCaseInsensitive,
    nsFNameExt = nsfCaseInsensitive | nsfFNameExt,
    nsRegularExpression = nsfRegularExpression | nsfCaseSensitive,
    nsCaseInRegularExpression = nsfRegularExpression | nsfCaseInsensitive
};
