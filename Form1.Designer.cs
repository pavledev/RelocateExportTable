
namespace RelocateExportTable
{
    partial class Form1
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.btnConvertDllToLib = new System.Windows.Forms.Button();
            this.btnDisplayExportTable = new System.Windows.Forms.Button();
            this.btnChangePath = new System.Windows.Forms.Button();
            this.btnModifyDLL = new System.Windows.Forms.Button();
            this.txtPDBPath = new System.Windows.Forms.TextBox();
            this.lblPDBPath = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.rtbClasses = new System.Windows.Forms.RichTextBox();
            this.rtbClasses2 = new System.Windows.Forms.RichTextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.btnLoadFromFile = new System.Windows.Forms.Button();
            this.btnLoadFromExportTable = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // btnConvertDllToLib
            // 
            this.btnConvertDllToLib.Font = new System.Drawing.Font("Microsoft Sans Serif", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnConvertDllToLib.Location = new System.Drawing.Point(583, 720);
            this.btnConvertDllToLib.Name = "btnConvertDllToLib";
            this.btnConvertDllToLib.Size = new System.Drawing.Size(219, 42);
            this.btnConvertDllToLib.TabIndex = 43;
            this.btnConvertDllToLib.Text = "Convert Dll to Lib";
            this.btnConvertDllToLib.UseVisualStyleBackColor = true;
            this.btnConvertDllToLib.Click += new System.EventHandler(this.BtnConvertDllToLib_Click);
            // 
            // btnDisplayExportTable
            // 
            this.btnDisplayExportTable.Font = new System.Drawing.Font("Microsoft Sans Serif", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnDisplayExportTable.Location = new System.Drawing.Point(808, 718);
            this.btnDisplayExportTable.Name = "btnDisplayExportTable";
            this.btnDisplayExportTable.Size = new System.Drawing.Size(201, 46);
            this.btnDisplayExportTable.TabIndex = 41;
            this.btnDisplayExportTable.Text = "Display export table";
            this.btnDisplayExportTable.UseVisualStyleBackColor = true;
            this.btnDisplayExportTable.Click += new System.EventHandler(this.BtnDisplayExportTable_Click);
            // 
            // btnChangePath
            // 
            this.btnChangePath.Font = new System.Drawing.Font("Microsoft Sans Serif", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnChangePath.Location = new System.Drawing.Point(1236, 16);
            this.btnChangePath.Name = "btnChangePath";
            this.btnChangePath.Size = new System.Drawing.Size(109, 39);
            this.btnChangePath.TabIndex = 29;
            this.btnChangePath.Text = "Change";
            this.btnChangePath.UseVisualStyleBackColor = true;
            this.btnChangePath.Click += new System.EventHandler(this.BtnChangePath_Click);
            // 
            // btnModifyDLL
            // 
            this.btnModifyDLL.Font = new System.Drawing.Font("Microsoft Sans Serif", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnModifyDLL.Location = new System.Drawing.Point(358, 720);
            this.btnModifyDLL.Name = "btnModifyDLL";
            this.btnModifyDLL.Size = new System.Drawing.Size(219, 42);
            this.btnModifyDLL.TabIndex = 28;
            this.btnModifyDLL.Text = "Modify Dll";
            this.btnModifyDLL.UseVisualStyleBackColor = true;
            this.btnModifyDLL.Click += new System.EventHandler(this.BtnModifyDLL_Click);
            // 
            // txtPDBPath
            // 
            this.txtPDBPath.Font = new System.Drawing.Font("Microsoft Sans Serif", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtPDBPath.Location = new System.Drawing.Point(111, 20);
            this.txtPDBPath.Name = "txtPDBPath";
            this.txtPDBPath.Size = new System.Drawing.Size(1119, 29);
            this.txtPDBPath.TabIndex = 24;
            // 
            // lblPDBPath
            // 
            this.lblPDBPath.AutoSize = true;
            this.lblPDBPath.Font = new System.Drawing.Font("Microsoft Sans Serif", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblPDBPath.Location = new System.Drawing.Point(12, 23);
            this.lblPDBPath.Name = "lblPDBPath";
            this.lblPDBPath.Size = new System.Drawing.Size(93, 24);
            this.lblPDBPath.TabIndex = 23;
            this.lblPDBPath.Text = "PDB path:";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label1.Location = new System.Drawing.Point(30, 103);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(158, 24);
            this.label1.TabIndex = 44;
            this.label1.Text = "Classes to export:";
            // 
            // rtbClasses
            // 
            this.rtbClasses.Font = new System.Drawing.Font("Microsoft Sans Serif", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.rtbClasses.Location = new System.Drawing.Point(34, 130);
            this.rtbClasses.Name = "rtbClasses";
            this.rtbClasses.Size = new System.Drawing.Size(620, 584);
            this.rtbClasses.TabIndex = 45;
            this.rtbClasses.Text = "";
            // 
            // rtbClasses2
            // 
            this.rtbClasses2.Font = new System.Drawing.Font("Microsoft Sans Serif", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.rtbClasses2.Location = new System.Drawing.Point(693, 130);
            this.rtbClasses2.Name = "rtbClasses2";
            this.rtbClasses2.Size = new System.Drawing.Size(620, 584);
            this.rtbClasses2.TabIndex = 46;
            this.rtbClasses2.Text = "";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Font = new System.Drawing.Font("Microsoft Sans Serif", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label2.Location = new System.Drawing.Point(689, 103);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(456, 24);
            this.label2.TabIndex = 47;
            this.label2.Text = "Classes (for modifying existing functions to be public):";
            // 
            // btnLoadFromFile
            // 
            this.btnLoadFromFile.Font = new System.Drawing.Font("Microsoft Sans Serif", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnLoadFromFile.Location = new System.Drawing.Point(133, 720);
            this.btnLoadFromFile.Name = "btnLoadFromFile";
            this.btnLoadFromFile.Size = new System.Drawing.Size(219, 42);
            this.btnLoadFromFile.TabIndex = 48;
            this.btnLoadFromFile.Text = "Load from file";
            this.btnLoadFromFile.UseVisualStyleBackColor = true;
            this.btnLoadFromFile.Click += new System.EventHandler(this.BtnLoadFromFile_Click);
            // 
            // btnLoadFromExportTable
            // 
            this.btnLoadFromExportTable.Font = new System.Drawing.Font("Microsoft Sans Serif", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnLoadFromExportTable.Location = new System.Drawing.Point(1015, 720);
            this.btnLoadFromExportTable.Name = "btnLoadFromExportTable";
            this.btnLoadFromExportTable.Size = new System.Drawing.Size(219, 42);
            this.btnLoadFromExportTable.TabIndex = 49;
            this.btnLoadFromExportTable.Text = "Load from export table";
            this.btnLoadFromExportTable.UseVisualStyleBackColor = true;
            this.btnLoadFromExportTable.Click += new System.EventHandler(this.BtnLoadFromExportTable_Click);
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1357, 774);
            this.Controls.Add(this.btnLoadFromExportTable);
            this.Controls.Add(this.btnLoadFromFile);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.rtbClasses2);
            this.Controls.Add(this.rtbClasses);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.btnConvertDllToLib);
            this.Controls.Add(this.btnDisplayExportTable);
            this.Controls.Add(this.btnChangePath);
            this.Controls.Add(this.btnModifyDLL);
            this.Controls.Add(this.txtPDBPath);
            this.Controls.Add(this.lblPDBPath);
            this.Name = "Form1";
            this.Text = "Form1";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button btnConvertDllToLib;
        private System.Windows.Forms.Button btnDisplayExportTable;
        private System.Windows.Forms.Button btnChangePath;
        private System.Windows.Forms.Button btnModifyDLL;
        private System.Windows.Forms.TextBox txtPDBPath;
        private System.Windows.Forms.Label lblPDBPath;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.RichTextBox rtbClasses;
        private System.Windows.Forms.RichTextBox rtbClasses2;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Button btnLoadFromFile;
        private System.Windows.Forms.Button btnLoadFromExportTable;
    }
}

