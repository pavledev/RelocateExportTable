using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Dia2Lib;
using PeNet;
using PeNet.Header.Pe;

namespace RelocateExportTable
{
    public partial class Form1 : Form
    {
        IDiaSymbol global;
        uint exportTableEnd;
        byte[] array;

        [DllImport("undname.dll", CharSet = CharSet.Ansi, CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl)]
        static extern IntPtr undecorateName([MarshalAs(UnmanagedType.LPStr)] string input, int flags);

        public Form1()
        {
            InitializeComponent();
        }

        private void BtnChangePath_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                Filter = "PDB Files (*.PDB) | *.pdb;"
            };

            DialogResult dialogResult = openFileDialog.ShowDialog();

            if (dialogResult == DialogResult.OK)
            {
                txtPDBPath.Text = openFileDialog.FileName;

                rtbClasses.Clear();
                rtbClasses2.Clear();

                LoadPDBData();
            }
        }

        private void BtnLoadFromFile_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                Filter = "Text Files (*.TXT) | *.txt;"
            };

            DialogResult dialogResult = openFileDialog.ShowDialog();

            if (dialogResult == DialogResult.OK)
            {
                string[] lines = File.ReadAllLines(openFileDialog.FileName);

                rtbClasses.Lines = lines;
            }
        }

        private void BtnModifyDLL_Click(object sender, EventArgs e)
        {
            if (txtPDBPath.Text.Length == 0)
            {
                return;
            }

            if (rtbClasses2.Text.Length > 0)
            {
                ModifyExistingFunctionsToBePublic();
            }

            int classesCount = rtbClasses.Lines.Length;

            if (classesCount == 0)
            {
                return;
            }

            List<Function> functions = new List<Function>();
            List<string> classes = new List<string>();

            for (int i = 0; i < classesCount; i++)
            {
                string className = rtbClasses.Lines[i];

                if (className.Length == 0 || CheckIfClassIsInExportTable(className))
                {
                    continue;
                }

                functions.AddRange(GetFunctionsForClass(className));

                if (functions == null)
                {
                    classes.Add(className);
                }
            }

            uint sizeNeededForExports = 0;

            foreach (Function function in functions)
            {
                sizeNeededForExports += (uint)(function.MangledName.Length + 11);
            }

            string path = Path.ChangeExtension(txtPDBPath.Text, "dll");

            AddExportTableSection(path, sizeNeededForExports);

            foreach (Function function in functions)
            {
                if (function.Access != Access.Public)
                {
                    ModifyFunctionToBePublic(function);
                }

                AddFunctionToExportTable(function.MangledName, function.RelativeVirtualAddress.ToString("X"));
            }

            string outputFilePath = GetOutputFilePath();

            File.WriteAllBytes(outputFilePath, array);

            rtbClasses.Clear();
            rtbClasses2.Clear();

            string text = "Done.";
            int classesCount2 = classes.Count;

            if (classesCount2 > 0)
            {
                text += " Functions of classes below weren't modified:\n";
            }

            for (int i = 0; i < classesCount2; i++)
            {
                text += classes[i] + "\n";
            }

            MessageBox.Show(text, "Relocate Export Table", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void BtnConvertDllToLib_Click(object sender, EventArgs e)
        {
            ConvertDllToLib();
        }

        private void BtnDisplayExportTable_Click(object sender, EventArgs e)
        {
            if (txtPDBPath.Text.Length == 0)
            {
                return;
            }

            ExportTable exportTable = new ExportTable(this)
            {
                FilePath = Path.ChangeExtension(txtPDBPath.Text, "dll")
            };

            exportTable.Show();
        }

        private void BtnLoadFromExportTable_Click(object sender, EventArgs e)
        {
            string filePath = Path.ChangeExtension(txtPDBPath.Text, "dll");
            PeFile peFile = new PeFile(filePath);
            ImageExportDirectory exportDir = peFile.ImageExportDirectory;
            var sectionHeaders = peFile.ImageSectionHeaders;
            var addressOfNamesOffset = exportDir.AddressOfNames.RvaToOffset(sectionHeaders);
            uint numberOfFunctions = exportDir.NumberOfFunctions;
            var rawFile = peFile.RawFile;
            string className = "";
            List<string> classes = new List<string>();

            for (uint i = 0; i < 4 * numberOfFunctions; i += 4)
            {
                var nameOffset = rawFile.ReadUInt(addressOfNamesOffset + i).RvaToOffset(sectionHeaders);
                string mangledName = rawFile.ReadAsciiString(nameOffset);
                IntPtr ptr = undecorateName(mangledName, (int)UnDecorateSymbolNameFlags.NameOnly);
                string demangledName = Marshal.PtrToStringAnsi(ptr);

                if (demangledName != null && demangledName.Contains("::"))
                {
                    string className2 = demangledName.Substring(0, demangledName.LastIndexOf("::"));

                    if (className != className2)
                    {
                        className = className2;

                        classes.Add(className);
                    }
                }
            }

            rtbClasses2.Lines = classes.ToArray();
        }

        private void LoadPDBData()
        {
            IDiaDataSource dataSource = new DiaSource();

            dataSource.loadDataFromPdb(txtPDBPath.Text);
            dataSource.openSession(out IDiaSession session);

            global = session.globalScope;
        }

        public List<Function> GetFunctionsForClass(string className)
        {
            List<Function> functions = new List<Function>();
            NameSearchOptions searchOptions = NameSearchOptions.nsfUndecoratedName | NameSearchOptions.nsfRegularExpression;
            string name = string.Format("* {0}::*", className);
            List<Function> memberFunctions = GetMemberFunctions(className);

            global.findChildren(SymTagEnum.SymTagPublicSymbol, name, (uint)searchOptions, out IDiaEnumSymbols publicSymbols);

            int i = 0;
            int count = publicSymbols.count;

            while (i < count)
            {
                publicSymbols.Next(1, out IDiaSymbol publicSymbol, out uint pceltFetched);

                if (pceltFetched == 0)
                {
                    break;
                }

                string fullDemangledName = publicSymbol.undecoratedName;
                bool isFunctionConst = false, isFunctionVolatile = false;

                if (fullDemangledName.EndsWith("const "))
                {
                    isFunctionConst = true;
                }
                else if (fullDemangledName.EndsWith("volatile "))
                {
                    isFunctionVolatile = true;
                }

                fullDemangledName = fullDemangledName.Replace("class ", "");
                fullDemangledName = fullDemangledName.Replace("struct ", "");
                fullDemangledName = fullDemangledName.Replace("union ", "");
                fullDemangledName = fullDemangledName.Replace("enum ", "");

                if (fullDemangledName.Contains(", const"))
                {
                    fullDemangledName = fullDemangledName.Replace(" const", " ");
                }

                if (fullDemangledName.Contains(" const "))
                {
                    fullDemangledName = fullDemangledName.Replace(" const ", " ");
                }

                if (fullDemangledName.Contains(" const"))
                {
                    fullDemangledName = fullDemangledName.Replace(" const", "");
                }

                if (fullDemangledName.Contains("const "))
                {
                    fullDemangledName = fullDemangledName.Replace("const ", "");
                }

                if (fullDemangledName.Contains(", volatile"))
                {
                    fullDemangledName = fullDemangledName.Replace(" volatile", " ");
                }

                if (fullDemangledName.Contains(" volatile "))
                {
                    fullDemangledName = fullDemangledName.Replace(" volatile ", " ");
                }

                if (fullDemangledName.Contains(" volatile"))
                {
                    fullDemangledName = fullDemangledName.Replace(" volatile", "");
                }

                if (fullDemangledName.Contains("volatile "))
                {
                    fullDemangledName = fullDemangledName.Replace("volatile ", "");
                }

                if (isFunctionConst)
                {
                    fullDemangledName += "const ";
                }
                else if (isFunctionVolatile)
                {
                    fullDemangledName += "volatile ";
                }

                fullDemangledName = fullDemangledName.Replace("__int8", "char");
                fullDemangledName = fullDemangledName.Replace("__int16", "short");
                fullDemangledName = fullDemangledName.Replace("__int32", "int");
                fullDemangledName = fullDemangledName.Replace("__int64", "long long");

                foreach (Function function in memberFunctions)
                {
                    if (fullDemangledName == function.FullDemangledName)
                    {
                        function.MangledName = publicSymbol.name;

                        functions.Add(function);

                        break;
                    }
                }

                i++;
            }

            if (memberFunctions.Count == functions.Count)
            {
                return functions;
            }

            return null;
        }

        private void AddFunctionToExportTable(string mangledName, string functionRVA)
        {
            PeFile peFile = new PeFile(array);
            var exportDir = peFile.ImageExportDirectory;
            var rawFile = peFile.RawFile;
            var sectionHeaders = peFile.ImageSectionHeaders;
            uint numberOfFunctions = exportDir.NumberOfFunctions;
            var ind1 = exportDir.AddressOfNames.RvaToOffset(sectionHeaders);
            uint ind2 = exportTableEnd;

            byte[] temp1 = rawFile.AsSpan(ind1, ind2 - ind1).ToArray();
            uint newAddressOfFunction = Convert.ToUInt32(functionRVA, 16);

            rawFile.WriteUInt(ind1, newAddressOfFunction);
            rawFile.WriteBytes(ind1 + 4, temp1);

            ind1 += 4;

            for (uint i = 0; i < numberOfFunctions; i++)
            {
                uint addressOfName = rawFile.ReadUInt(ind1 + 4 * i) + 10;

                rawFile.WriteUInt(ind1 + 4 * i, addressOfName);
            }

            exportDir.AddressOfNames += 4;
            ind1 = exportDir.AddressOfNameOrdinals.RvaToOffset(sectionHeaders) + 4;
            ind2 += 4;

            byte[] temp2 = rawFile.AsSpan(ind1, ind2 - ind1).ToArray();

            rawFile.WriteUInt(ind1, ind2.OffsetToRva(sectionHeaders) + 6);
            rawFile.WriteBytes(ind1 + 4, temp2);

            exportDir.AddressOfNameOrdinals += 8;
            ind1 = exportDir.Name.RvaToOffset(sectionHeaders) + 8;
            ind2 += 4;

            byte[] temp3 = rawFile.AsSpan(ind1, ind2 - ind1).ToArray();
            ushort ordinal = (ushort)exportDir.NumberOfFunctions;

            rawFile.WriteUShort(ind1, ordinal);
            rawFile.WriteBytes(ind1 + 2, temp3);

            exportDir.Name += 10;
            exportDir.NumberOfFunctions++;
            exportDir.NumberOfNames++;
            ind2 += 2;

            byte[] nameBytes = Encoding.ASCII.GetBytes(mangledName);

            rawFile.WriteBytes(ind2, nameBytes);

            exportTableEnd = ind2 + (uint)nameBytes.Length + 1;

            var temp = peFile.ImageSectionHeaders.OrderByDescending(sh => sh.VirtualAddress).First();

            peFile.ImageNtHeaders.OptionalHeader.SizeOfImage = temp.VirtualAddress + temp.VirtualSize;
        }

        private void AddExportTableSection(string fileName, uint newSize)
        {
            var peFile = new PeFile(fileName);

            Array.Resize(ref array, (int)peFile.FileSize);

            var rawFile = peFile.RawFile;
            array = rawFile.ToArray();
            var sectionHeaders = peFile.ImageSectionHeaders;
            uint oldExportTableRVA = peFile.ImageNtHeaders.OptionalHeader.DataDirectory[0].VirtualAddress;
            uint lastSectionRVA = peFile.ImageSectionHeaders.OrderByDescending(sh => sh.VirtualAddress).First().VirtualAddress;
            uint oldExportTableOffset = oldExportTableRVA.RvaToOffset(sectionHeaders);
            uint ind1 = oldExportTableOffset;
            var exportDir = peFile.ImageExportDirectory;
            uint numberOfFunctions = exportDir.NumberOfFunctions;
            var exportFunctions = peFile.ExportedFunctions;
            var addressOfLastNameLocation = exportDir.AddressOfNameOrdinals.RvaToOffset(sectionHeaders) - 4;
            var addressOfLastName = rawFile.ReadUInt(addressOfLastNameLocation).RvaToOffset(sectionHeaders);

            string lastFunctionName = exportFunctions.Last().Name;
            uint ind2 = addressOfLastName + (uint)lastFunctionName.Length + 1;
            byte[] exportTable = rawFile.AsSpan(ind1, ind2 - ind1).ToArray();
            exportTableEnd = oldExportTableOffset + peFile.ImageNtHeaders.OptionalHeader.DataDirectory[0].Size;

            // Done with original (smaller) dll
            // Create new bigger file with room for all exports
            if (oldExportTableRVA < lastSectionRVA)
            {
                peFile.AddSection(".edata", (int)newSize + exportTable.Length, (ScnCharacteristicsType)0x40000040);

                uint newExportTableRVA = peFile.ImageSectionHeaders.OrderByDescending(sh => sh.VirtualAddress).First().VirtualAddress;

                peFile.ImageNtHeaders.OptionalHeader.DataDirectory[0].VirtualAddress = newExportTableRVA;
                peFile.ImageNtHeaders.OptionalHeader.DataDirectory[0].Size = (uint)exportTable.Length + newSize;

                byte[] array2 = rawFile.ToArray();
                var bigPeFile = new PeFile(array2);
                sectionHeaders = bigPeFile.ImageSectionHeaders;
                var bigRawFile = bigPeFile.RawFile;
                var exportTableOffset = newExportTableRVA.RvaToOffset(sectionHeaders);

                bigRawFile.WriteBytes(exportTableOffset, exportTable);
                exportTableEnd = exportTableOffset + (uint)exportTable.Length;

                // adjust values to new location
                uint exportTableFactor = newExportTableRVA - oldExportTableRVA;

                exportDir = bigPeFile.ImageExportDirectory;
                exportDir.Name += exportTableFactor;
                exportDir.AddressOfFunctions += exportTableFactor;
                exportDir.AddressOfNames += exportTableFactor;
                exportDir.AddressOfNameOrdinals += exportTableFactor;

                var addressOfNamesOffset = exportDir.AddressOfNames.RvaToOffset(sectionHeaders);

                for (uint i = 0; i < 4 * numberOfFunctions; i += 4)
                {
                    var nameAddress = bigRawFile.ReadUInt(addressOfNamesOffset + i) + exportTableFactor;

                    bigRawFile.WriteUInt(addressOfNamesOffset + i, nameAddress);
                }

                Array.Resize(ref array, array2.Length);

                array = array2;
            }
            else
            {
                Array.Resize(ref array, array.Length + (int)newSize);

                PeFile bigPeFile = new PeFile(array);

                bigPeFile.ImageNtHeaders.OptionalHeader.DataDirectory[0].Size += newSize;
                bigPeFile.ImageSectionHeaders.OrderByDescending(sh => sh.VirtualAddress).First().SizeOfRawData += newSize;
                bigPeFile.ImageSectionHeaders.OrderByDescending(sh => sh.VirtualAddress).First().VirtualSize += newSize;
            }
        }

        private uint GetSymbolID(string symbolName)
        {
            global.findChildren(SymTagEnum.SymTagUDT, symbolName, 0, out IDiaEnumSymbols udtSymbols);

            if (udtSymbols.count > 0)
            {
                return udtSymbols.Item(0).symIndexId;
            }

            return 0;
        }

        private List<Function> GetMemberFunctions(string className)
        {
            List<Function> memberFunctions = new List<Function>();

            global.findChildren(SymTagEnum.SymTagUDT, className, 0, out IDiaEnumSymbols udtSymbols);

            IDiaSymbol udtSymbol = udtSymbols.Item(0);

            udtSymbol.findChildren(SymTagEnum.SymTagFunction, null, 0, out IDiaEnumSymbols functionSymbols);

            int i = 0;
            int count = functionSymbols.count;

            while (i < count)
            {
                functionSymbols.Next(1, out IDiaSymbol functionSymbol, out uint pceltFetched);

                if (pceltFetched == 0)
                {
                    break;
                }

                if (functionSymbol.relativeVirtualAddress == 0)
                {
                    continue;
                }

                RecordType recordType = GetRecordType(functionSymbol);

                Function function = new Function()
                {
                    DemangledName = functionSymbol.name,
                    ParentClassName = functionSymbol.classParent.name,
                    Access = (Access)functionSymbol.access,
                    RelativeVirtualAddress = functionSymbol.relativeVirtualAddress,
                    IsStatic = Convert.ToBoolean(functionSymbol.isStatic),
                    IsVirtual = Convert.ToBoolean(functionSymbol.@virtual),
                    IsPure = Convert.ToBoolean(functionSymbol.pure),
                    ReturnType = recordType.FunctionReturnType,
                    CallingConvention = recordType.CallingConvention,
                    IsConst = recordType.IsFunctionConst,
                    IsVolatile = recordType.IsFunctionVolatile,
                    IsConstructor = functionSymbol.classParent.name == functionSymbol.name,
                    IsDestructor = functionSymbol.name.StartsWith("~"),
                    IsVariadic = recordType.IsVariadicFunction,
                    Parameters = recordType.FunctionParameters,
                    ReturnsFunctionPointer = recordType.FunctionReturnsFunctionPointer
                };

                function.FullDemangledName = GenerateFunctionPrototype(function);

                memberFunctions.Add(function);

                i++;
            }

            return memberFunctions;
        }

        private bool CheckIfClassIsInExportTable(string className)
        {
            string filePath = Path.ChangeExtension(txtPDBPath.Text, "dll");
            PeFile peFile = new PeFile(filePath);
            ExportFunction[] exportFunctions = peFile.ExportedFunctions;

            foreach (ExportFunction exportFunction in exportFunctions)
            {
                IntPtr ptr = undecorateName(exportFunction.Name, (int)UnDecorateSymbolNameFlags.NameOnly);
                string demangledName = Marshal.PtrToStringAnsi(ptr);

                int index = demangledName.LastIndexOf("::");

                if (index > 0 && demangledName.Substring(0, index).Equals(className))
                {
                    return true;
                }
            }

            return false;
        }

        private void ModifyExistingFunctionsToBePublic()
        {
            string filePath = Path.ChangeExtension(txtPDBPath.Text, "dll");
            PeFile peFile = new PeFile(filePath);
            ImageExportDirectory exportDir = peFile.ImageExportDirectory;
            var sectionHeaders = peFile.ImageSectionHeaders;
            var addressOfNamesOffset = exportDir.AddressOfNames.RvaToOffset(sectionHeaders);
            uint numberOfFunctions = exportDir.NumberOfFunctions;
            var rawFile = peFile.RawFile;
            Dictionary<string, uint> mangledNames = new Dictionary<string, uint>();
            List<string> classes = new List<string>();

            for (uint i = 0; i < 4 * numberOfFunctions; i += 4)
            {
                var nameOffset = rawFile.ReadUInt(addressOfNamesOffset + i).RvaToOffset(sectionHeaders);
                string mangledName = rawFile.ReadAsciiString(nameOffset);

                mangledNames.Add(mangledName, nameOffset);
            }

            int classesCount = rtbClasses2.Lines.Length;

            for (int i = 0; i < classesCount; i++)
            {
                string className = rtbClasses2.Lines[i];

                global.findChildren(SymTagEnum.SymTagUDT, className, 0, out IDiaEnumSymbols udtSymbols);

                if (udtSymbols.count == 0)
                {
                    continue;
                }

                List<Function> functions = GetFunctionsForClass(className);

                if (functions == null)
                {
                    //MessageBox.Show("Some mangled names weren't matched with functions.", "Relocate Export Table", MessageBoxButtons.OK, MessageBoxIcon.Error);

                    //return;

                    classes.Add(className);

                    continue;
                }

                foreach (Function function in functions)
                {
                    if (function.Access != Access.Public && mangledNames.ContainsKey(function.MangledName))
                    {
                        uint nameOffset = mangledNames[function.MangledName];

                        ModifyFunctionToBePublic(function);

                        rawFile.WriteBytes(nameOffset, Encoding.ASCII.GetBytes(function.MangledName));
                    }
                }
            }

            string outputFilePath = GetOutputFilePath();
            byte[] array = rawFile.ToArray();

            File.WriteAllBytes(outputFilePath, array);

            string text = "Done.";
            int classesCount2 = classes.Count;

            if (classesCount2 > 0)
            {
                text += " Functions of classes below weren't modified:\n";
            }

            for (int i = 0; i < classesCount2; i++)
            {
                text += classes[i] + "\n";
            }

            MessageBox.Show(text, "Relocate Export Table", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        public void ModifyFunctionToBePublic(Function function)
        {
            char letter = ' ';
            char newLetter;

            if (function.IsVirtual)
            {
                if (function.Access == Access.Private)
                {
                    letter = 'E';
                }
                else if (function.Access == Access.Protected)
                {
                    letter = 'M';
                }

                newLetter = 'U';
            }
            else if (function.IsStatic)
            {
                if (function.Access == Access.Private)
                {
                    letter = 'C';
                }
                else if (function.Access == Access.Protected)
                {
                    letter = 'K';
                }

                newLetter = 'S';
            }
            else
            {
                if (function.Access == Access.Private)
                {
                    letter = 'A';
                }
                else if (function.Access == Access.Protected)
                {
                    letter = 'I';
                }

                newLetter = 'Q';
            }

            int index = function.MangledName.IndexOf("@@" + letter);

            function.MangledName = function.MangledName.Remove(index + 2, 1).Insert(index + 2, newLetter.ToString());
        }

        public string GetOutputFilePath()
        {
            return Path.ChangeExtension(txtPDBPath.Text, "dll");
        }

        private string GenerateFunctionPrototype(Function function)
        {
            string prototype = "";

            switch (function.Access)
            {
                case Access.Private:
                    prototype += "private";

                    break;
                case Access.Protected:
                    prototype += "protected";

                    break;
                case Access.Public:
                    prototype += "public";

                    break;
            }

            if (function.Access != 0)
            {
                prototype += ": ";
            }

            if (function.IsVirtual)
            {
                prototype += "virtual ";
            }
            else if (function.IsStatic)
            {
                prototype += "static ";
            }

            if (!function.IsConstructor && !function.IsDestructor)
            {
                prototype += function.ReturnType + " ";
            }

            prototype += ConvertCallingConventionToString(function.CallingConvention) + " ";
            prototype += function.ParentClassName + "::" + function.DemangledName;
            prototype += "(";

            int parametersCount = function.Parameters.Count;

            if (parametersCount > 0)
            {
                for (int i = 0; i < parametersCount; i++)
                {
                    prototype += function.Parameters[i];

                    if (i != parametersCount - 1)
                    {
                        prototype += ",";
                    }
                }
            }
            else
            {
                prototype += "void";
            }

            prototype += ")";

            if (function.IsConst)
            {
                prototype += "const ";
            }

            if (function.IsVolatile)
            {
                prototype += "volatile ";
            }

            return prototype;
        }

        private RecordType GetRecordType(IDiaSymbol symbol)
        {
            RecordType recordType = new RecordType();
            IDiaSymbol type = symbol.type;

            if (type != null)
            {
                return GetType(type);
            }

            return recordType;
        }

        private RecordType GetType(IDiaSymbol symbol)
        {
            RecordType recordType = new RecordType();
            SymTagEnum symTag = (SymTagEnum)symbol.symTag;

            if (symbol == null)
            {
                return recordType;
            }

            switch (symTag)
            {
                case SymTagEnum.SymTagBaseType:
                    {
                        BaseType baseType = GetBaseType(symbol);

                        recordType.Size = baseType.Length;
                        recordType.IsTypeConst = baseType.IsConst;
                        recordType.IsTypeVolatile = baseType.IsVolatile;
                        recordType.BaseType = baseType.Type;

                        switch (recordType.BaseType)
                        {
                            case 0:
                                recordType.TypeName = "<btNoType>";
                                recordType.NoType = true;

                                break;
                            case 1: recordType.TypeName = "void"; break;
                            case 2: recordType.TypeName = "char"; break;
                            case 3: recordType.TypeName = "wchar_t"; break;
                            case 4: recordType.TypeName = "signed char"; break;
                            case 5: recordType.TypeName = "unsigned char"; break;
                            case 6: recordType.TypeName = "int"; break;
                            case 7: recordType.TypeName = "unsigned int"; break;
                            case 8: recordType.TypeName = "float"; break;
                            case 9: recordType.TypeName = "BCD"; break;
                            case 10: recordType.TypeName = "bool"; break;
                            case 11: recordType.TypeName = "short"; break;
                            case 12: recordType.TypeName = "unsigned short"; break;
                            case 13: recordType.TypeName = "long"; break;
                            case 14: recordType.TypeName = "unsigned long"; break;
                            case 15: recordType.TypeName = "__int8"; break;
                            case 16: recordType.TypeName = "__int16"; break;
                            case 17: recordType.TypeName = "__int32"; break;
                            case 18: recordType.TypeName = "__int64"; break;
                            case 19: recordType.TypeName = "__int128"; break;
                            case 20: recordType.TypeName = "unsigned __int8"; break;
                            case 21: recordType.TypeName = "unsigned __int16"; break;
                            case 22: recordType.TypeName = "unsigned __int32"; break;
                            case 23: recordType.TypeName = "unsigned __int64"; break;
                            case 24: recordType.TypeName = "unsigned __int128"; break;
                            case 25: recordType.TypeName = "CURRENCY"; break;
                            case 26: recordType.TypeName = "DATE"; break;
                            case 27: recordType.TypeName = "VARIANT"; break;
                            case 28: recordType.TypeName = "COMPLEX"; break;
                            case 29: recordType.TypeName = "BIT"; break;
                            case 30: recordType.TypeName = "BSTR"; break;
                            case 31: recordType.TypeName = "HRESULT"; break;
                            case 32: recordType.TypeName = "char16_t"; break;
                            case 33: recordType.TypeName = "char32_t"; break;
                        }

                        if ((recordType.BaseType == 8) && (recordType.Size != 4))
                        {
                            switch (recordType.Size)
                            {
                                case 8: recordType.TypeName = "double"; break;
                                case 12: recordType.TypeName = "long double"; break;
                            }
                        }

                        if (((recordType.BaseType == 7) || (recordType.BaseType == 14)) && (recordType.Size != 4)) // "unsigned int"
                        {
                            switch (recordType.Size)
                            {
                                case 1: recordType.TypeName = "unsigned char"; break;
                                case 2: recordType.TypeName = "unsigned short"; break;
                                case 4: recordType.TypeName = "unsigned int"; break;
                                case 8: recordType.TypeName = "unsigned long long"; break;
                            }
                        }

                        if (((recordType.BaseType == 6) || (recordType.BaseType == 13)) && (recordType.Size != 4)) // "int"
                        {
                            switch (recordType.Size)
                            {
                                case 1: recordType.TypeName = "char"; break;
                                case 2: recordType.TypeName = "short"; break;
                                case 4: recordType.TypeName = "int"; break;
                                case 8: recordType.TypeName = "long long"; break;
                            }
                        }

                        break;
                    }
                case SymTagEnum.SymTagUDT:
                    {
                        UDT udt = GetUDT(symbol);

                        recordType.Size = udt.Length;
                        recordType.IsTypeConst = udt.IsConst;
                        recordType.IsTypeVolatile = udt.IsVolatile;
                        recordType.TypeName = udt.Name;
                        recordType.Type = udt.Type;
                        recordType.NoType = false;

                        break;
                    }
                case SymTagEnum.SymTagPointerType:
                    {
                        PointerType pointerType = GetPointerType(symbol);
                        IDiaSymbol type = symbol.type;

                        if (type != null)
                        {
                            recordType = GetType(type);

                            /*
                            * Don't check if data type is const or volatile in case of reference
                            * because value of reference can't be changed so references are already treated as const
                            */
                            if (pointerType.IsReference)
                            {
                                recordType.IsReference = true;
                                recordType.ReferenceLevel++;
                            }
                            else
                            {
                                recordType.IsPointer = true;
                                recordType.IsPointerConst = pointerType.IsConst;
                                recordType.IsPointerVolatile = pointerType.IsVolatile;
                                recordType.PointerLevel++;
                            }

                            recordType.Size = pointerType.Length;
                        }

                        break;
                    }
                case SymTagEnum.SymTagArrayType:
                    {
                        ArrayType arrayType = GetArrayType(symbol);
                        IDiaSymbol type = symbol.type;

                        if (type != null)
                        {
                            recordType = GetType(type);
                            recordType.IsArray = true;
                            recordType.ArrayCount.Add(arrayType.Count);
                            recordType.Size *= arrayType.Count;

                            recordType.IsTypeConst = arrayType.IsConst;
                            recordType.IsTypeVolatile = arrayType.IsVolatile;
                        }

                        break;
                    }
                case SymTagEnum.SymTagEnum:
                    {
                        Enum enumType = GetEnum(symbol);

                        recordType.Size = enumType.Length;
                        recordType.IsTypeConst = enumType.IsConst;
                        recordType.IsTypeVolatile = enumType.IsVolatile;
                        recordType.TypeName = enumType.Name;
                        recordType.Type = "enum";
                        recordType.NoType = false;

                        break;
                    }
                case SymTagEnum.SymTagFunctionType:
                    {
                        RecordType returnType = GetRecordType(symbol);
                        FunctionType functionType = GetFunctionType(symbol);

                        returnType.CallingConvention = functionType.CallingConvention;

                        if (returnType.Size > 0 && returnType.FunctionType)
                        {
                            returnType.IsFunctionPointer = true;
                        }

                        Data data = ConvertRecordTypeToData(ref returnType);

                        recordType.FunctionReturnType = DataTypeToString(ref data);
                        recordType.IsFunctionConst = functionType.IsConst;
                        recordType.IsFunctionVolatile = functionType.IsVolatile;
                        recordType.FunctionType = true;
                        recordType.CallingConvention = functionType.CallingConvention;

                        if (recordType.Size > 0)
                        {
                            recordType.IsFunctionPointer = true;
                        }

                        if (returnType.IsFunctionPointer)
                        {
                            recordType.FunctionReturnsFunctionPointer = true;
                        }

                        symbol.findChildren(SymTagEnum.SymTagNull, null, 0, out IDiaEnumSymbols enumSymbols);

                        int i = 0;
                        int count = enumSymbols.count;

                        while (i < count)
                        {
                            enumSymbols.Next(1, out IDiaSymbol symbol2, out uint celt);

                            RecordType recordType2 = GetRecordType(symbol2);

                            if (recordType2.Size > 0 && recordType2.FunctionType)
                            {
                                recordType2.IsFunctionPointer = true;
                            }

                            if (recordType2.NoType)
                            {
                                recordType.IsVariadicFunction = true;
                            }

                            Data data2 = ConvertRecordTypeToData(ref recordType2);
                            string parameter = DataTypeToString(ref data2);

                            recordType.FunctionParameters.Add(parameter);

                            i++;
                        }

                        break;
                    }
                default:
                    MessageBox.Show("Unknown Type.", "Relocate Export Table", MessageBoxButtons.OK, MessageBoxIcon.Error);

                    break;
            }

            return recordType;
        }

        private BaseType GetBaseType(IDiaSymbol symbol)
        {
            BaseType baseType = new BaseType
            {
                Type = symbol.baseType,
                Length = symbol.length,
                IsConst = Convert.ToBoolean(symbol.constType),
                IsVolatile = Convert.ToBoolean(symbol.volatileType)
            };

            return baseType;
        }

        private UDT GetUDT(IDiaSymbol symbol)
        {
            UDT udt = new UDT();

            udt.Name = symbol.name;
            udt.UdtKind = (UdtKind)symbol.udtKind;

            switch (udt.UdtKind)
            {
                case UdtKind.UdtStruct:
                    udt.Type = "struct";
                    break;
                case UdtKind.UdtClass:
                    udt.Type = "class";
                    break;
                case UdtKind.UdtUnion:
                    udt.Type = "union";
                    break;
                case UdtKind.UdtInterface:
                    udt.Type = "interface";
                    break;
            }

            udt.Length = symbol.length;
            udt.IsConst = Convert.ToBoolean(symbol.constType);
            udt.IsVolatile = Convert.ToBoolean(symbol.volatileType);

            return udt;
        }

        private PointerType GetPointerType(IDiaSymbol symbol)
        {
            PointerType pointerType = new PointerType
            {
                Length = symbol.length,
                IsReference = Convert.ToBoolean(symbol.reference),
                IsConst = Convert.ToBoolean(symbol.constType),
                IsVolatile = Convert.ToBoolean(symbol.volatileType)
            };

            return pointerType;
        }

        private ArrayType GetArrayType(IDiaSymbol symbol)
        {
            ArrayType arrayType = new ArrayType
            {
                Count = symbol.count,
                Length = symbol.length,
                IsConst = Convert.ToBoolean(symbol.constType),
                IsVolatile = Convert.ToBoolean(symbol.volatileType)
            };

            return arrayType;
        }

        private Enum GetEnum(IDiaSymbol symbol)
        {
            Enum enum1 = new Enum
            {
                Name = symbol.name,
                BaseType = symbol.baseType,
                Length = symbol.length,
                IsConst = Convert.ToBoolean(symbol.constType),
                IsVolatile = Convert.ToBoolean(symbol.volatileType)
            };

            return enum1;
        }

        private FunctionType GetFunctionType(IDiaSymbol symbol)
        {
            FunctionType functionType = new FunctionType();

            functionType.CallingConvention = (CallingConvention)symbol.callingConvention;

            IDiaSymbol objectPointer = symbol.objectPointerType;

            if (objectPointer != null)
            {
                SymTagEnum symTag = (SymTagEnum)objectPointer.symTag;

                if (symTag == SymTagEnum.SymTagPointerType)
                {
                    IDiaSymbol pointee = objectPointer.type;

                    if (pointee != null)
                    {
                        functionType.IsConst = Convert.ToBoolean(pointee.constType);
                        functionType.IsVolatile = Convert.ToBoolean(pointee.volatileType);
                    }
                }
            }

            return functionType;
        }

        private Data ConvertRecordTypeToData(ref RecordType recordType)
        {
            Data data = new Data();

            data.BaseType = recordType.BaseType;
            data.IsTypeConst = recordType.IsTypeConst;
            data.IsTypeVolatile = recordType.IsTypeVolatile;
            data.IsPointerConst = recordType.IsPointerConst;
            data.IsPointerVolatile = recordType.IsPointerVolatile;
            data.TypeName = recordType.TypeName;
            data.Type = recordType.Type;
            data.IsPointer = recordType.IsPointer;
            data.IsReference = recordType.IsReference;
            data.PointerLevel = recordType.PointerLevel;
            data.ReferenceLevel = recordType.ReferenceLevel;
            data.IsArray = recordType.IsArray;
            data.ArrayCount = recordType.ArrayCount;
            data.FunctionReturnType = recordType.FunctionReturnType;
            data.FunctionParameters = recordType.FunctionParameters;
            data.IsVariadicFunction = recordType.IsVariadicFunction;
            data.CallingConvention = recordType.CallingConvention;
            data.IsFunctionPointer = recordType.IsFunctionPointer;
            data.NoType = recordType.NoType;

            return data;
        }

        private string DataTypeToString(ref Data data)
        {
            string result = "";

            //result += data.Type;

            //if (data.Type != null)
            //{
            //    result += " ";
            //}

            result += data.TypeName;

            //if (data.IsTypeConst)
            //{
            //    result += " const";

            //    if (!data.IsPointer && !data.IsReference)
            //    {
            //        result += " ";
            //    }
            //}

            //if (data.IsTypeVolatile)
            //{
            //    result += " volatile";

            //    if (!data.IsPointer && !data.IsReference)
            //    {
            //        result += " ";
            //    }
            //}

            if (data.IsFunctionPointer)
            {
                result += data.FunctionReturnType + " (";
            }

            if (data.IsPointer)
            {
                result += " ";

                for (int i = 0; i < data.PointerLevel; i++)
                {
                    result += "*";

                    if (i != data.PointerLevel - 1)
                    {
                        result += " ";
                    }
                }
            }

            if (data.IsReference)
            {
                result += " ";

                for (int i = 0; i < data.ReferenceLevel; i++)
                {
                    result += "&";

                    if (i != data.ReferenceLevel - 1)
                    {
                        result += " ";
                    }
                }
            }

            //if (data.IsPointerConst)
            //{
            //    result += " const";
            //}

            //if (data.IsPointerVolatile)
            //{
            //    result += " volatile";
            //}

            //if (!data.IsFunctionPointer &&
            //    result.Length > 0)
            //{
            //    result += " ";
            //}

            if (data.IsFunctionPointer)
            {
                result += ")";
            }

            if (data.IsArray)
            {
                int arrayCount = data.ArrayCount.Count;

                for (int i = arrayCount - 1; i >= 0; i--)
                {
                    result += "[" + data.ArrayCount[i] + "]";
                }
            }
            else if (data.IsFunctionPointer)
            {
                result += "(";

                int count = data.FunctionParameters.Count;

                if (data.IsVariadicFunction)
                {
                    count--;
                }

                for (int i = 0; i < count; i++)
                {
                    string parameterType = data.FunctionParameters[i];

                    result += parameterType;

                    if (i != count - 1)
                    {
                        result += ",";
                    }
                }

                if (data.IsVariadicFunction)
                {
                    if (count > 0)
                    {
                        result += ",";
                    }

                    result += "...";
                }

                result += ")";
            }

            if (result.Contains(", const"))
            {
                result = result.Replace(" const", " ");
            }

            if (result.Contains(" const "))
            {
                result = result.Replace(" const ", " ");
            }

            if (result.Contains(" const"))
            {
                result = result.Replace(" const", "");
            }

            if (result.Contains("const "))
            {
                result = result.Replace("const ", "");
            }

            if (result.Contains(", volatile"))
            {
                result = result.Replace(" volatile", " ");
            }

            if (result.Contains(" volatile "))
            {
                result = result.Replace(" volatile ", " ");
            }

            if (result.Contains(" volatile"))
            {
                result = result.Replace(" volatile", "");
            }

            if (result.Contains("volatile "))
            {
                result = result.Replace("volatile ", "");
            }

            return result;
        }

        private string ConvertCallingConventionToString(CallingConvention callingConvention)
        {
            string result = "";

            switch (callingConvention)
            {
                case CallingConvention.ThisCall:
                    result = "__thiscall";

                    break;
                case CallingConvention.NearFast:
                case CallingConvention.FarFast:
                    result = "__fastcall";

                    break;
                case CallingConvention.NearStdCall:
                case CallingConvention.FarStdCall:
                    result = "__stdcall";

                    break;
                case CallingConvention.NearCdecl:
                case CallingConvention.FarCdecl:
                    result = "__cdecl";

                    break;
                case CallingConvention.NearVector:
                    result = "__vectorcall";

                    break;
            }

            return result;
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            DialogResult dialogResult = MessageBox.Show("Do you want to convert dll to lib?", "Relocate Export Table", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            if (dialogResult == DialogResult.Yes)
            {
                ConvertDllToLib();
            }
        }

        private void ConvertDllToLib()
        {
            string fileName = Path.GetFileNameWithoutExtension(txtPDBPath.Text) + ".dll";
            Process process = new Process();
            ProcessStartInfo startInfo = new ProcessStartInfo
            {
                WindowStyle = ProcessWindowStyle.Hidden,
                WorkingDirectory = Path.GetDirectoryName(txtPDBPath.Text),
                FileName = "dll2lib.bat",
                Arguments = string.Format("{0} {1}", "32", fileName),
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true
            };

            process.StartInfo = startInfo;

            process.Start();

            string output = process.StandardOutput.ReadToEnd();
            string error = process.StandardError.ReadToEnd();

            process.WaitForExit();

            if (error.Length == 0)
            {
                MessageBox.Show(output);
            }
            else
            {
                MessageBox.Show(error);
            }
        }
    }
}
