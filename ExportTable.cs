using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using PeNet;
using PeNet.Header.Pe;
using PeNet.FileParser;
using System.Runtime.InteropServices;

namespace RelocateExportTable
{
    public partial class ExportTable : Form
    {
        private readonly Form1 form1;
        private List<ListViewItem> listViewItems;
        private bool modified;
        private PeFile peFile;
        private IRawFile rawFile;

        public string FilePath
        {
            get;
            set;
        }

        [DllImport("undname.dll", CharSet = CharSet.Ansi, CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl)]
        static extern IntPtr undecorateName([MarshalAs(UnmanagedType.LPStr)] string input, int flags);

        public ExportTable(Form1 form1)
        {
            InitializeComponent();

            this.form1 = form1;

            lvExportTable.FullRowSelect = true;
            lvExportTable.ContextMenuStrip = contextMenuStrip1;
        }

        private void ExportTable_Load(object sender, EventArgs e)
        {
            peFile = new PeFile(FilePath);
            ExportFunction[] exportFunctions = peFile.ExportedFunctions;

            rawFile = peFile.RawFile;
            listViewItems = new List<ListViewItem>(exportFunctions.Length);

            foreach (ExportFunction exportFunction in exportFunctions)
            {
                ListViewItem listViewItem = new ListViewItem("0x" + exportFunction.Ordinal.ToString("X"));

                listViewItem.SubItems.Add("0x" + exportFunction.Address.ToString("X"));

                IntPtr ptr = undecorateName(exportFunction.Name, (int)UnDecorateSymbolNameFlags.Complete);
                string demangledName = Marshal.PtrToStringAnsi(ptr);

                listViewItem.SubItems.Add(demangledName);
                listViewItems.Add(listViewItem);
            }

            lvExportTable.BeginUpdate();
            lvExportTable.Items.AddRange(listViewItems.ToArray());
            lvExportTable.EndUpdate();
        }

        private void TxtSearch_TextChanged(object sender, EventArgs e)
        {
            string name = txtSearch.Text;

            lvExportTable.Items.Clear();

            foreach (ListViewItem listViewItem in listViewItems)
            {
                string demangledName = listViewItem.SubItems[2].Text;

                if (demangledName.IndexOf(name, StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    lvExportTable.Items.Add(listViewItem);
                }
            }
        }

        private void TsmiChangeAcessSpecifierToPublic_Click(object sender, EventArgs e)
        {
            ImageExportDirectory exportDir = peFile.ImageExportDirectory;
            var sectionHeaders = peFile.ImageSectionHeaders;
            var addressOfNamesOffset = exportDir.AddressOfNames.RvaToOffset(sectionHeaders);
            int count = lvExportTable.SelectedIndices.Count;

            for (int i = 0; i < count; i++)
            {
                int index = lvExportTable.SelectedIndices[i];
                var nameOffset = rawFile.ReadUInt(addressOfNamesOffset + index * 4).RvaToOffset(sectionHeaders);
                string mangledName = rawFile.ReadAsciiString(nameOffset);
                IntPtr ptr = undecorateName(mangledName, (int)UnDecorateSymbolNameFlags.Complete);
                IntPtr ptr2 = undecorateName(mangledName, (int)UnDecorateSymbolNameFlags.NameOnly);
                string fullDemangledName = Marshal.PtrToStringAnsi(ptr);
                string demangledName = Marshal.PtrToStringAnsi(ptr2);

                if (demangledName.Contains("::"))
                {
                    string className = demangledName.Substring(0, demangledName.LastIndexOf("::"));
                    List<Function> functions = form1.GetFunctionsForClass(className);

                    foreach (Function function in functions)
                    {
                        if (function.FullDemangledName == fullDemangledName && function.Access != Access.Public)
                        {
                            form1.ModifyFunctionToBePublic(function);

                            rawFile.WriteBytes(nameOffset, Encoding.ASCII.GetBytes(function.MangledName));

                            modified = true;

                            break;
                        }
                    }
                }
            }
        }

        private void ExportTable_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (modified)
            {
                DialogResult dialogResult = MessageBox.Show("Are you sure that you want to modify functions?", "Relocate Export Table", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

                if (dialogResult == DialogResult.Yes)
                {
                    string outputFilePath = form1.GetOutputFilePath();
                    byte[] array = rawFile.ToArray();

                    File.WriteAllBytes(outputFilePath, array);
                }
            }
        }
    }
}
