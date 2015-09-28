using System;
using System.Collections;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Reflection;
using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Color=System.Drawing.Color;
using Rectangle=System.Drawing.Rectangle;


namespace tglrf
{
	/// <summary>
	/// Summary description for Form1.
	/// </summary>
    public class MainForm : System.Windows.Forms.Form
    {
        private System.Windows.Forms.TreeView levelTreeView;
        private System.Windows.Forms.PictureBox levelPictureBox;
        private System.Windows.Forms.MainMenu mainMenu1;
        private System.Windows.Forms.MenuItem menuItem1;
        private System.Windows.Forms.TabPage tabMultiPlayer;
        private System.Windows.Forms.TabPage tabLevel;
        private System.Windows.Forms.MenuItem menuFileSaveCurrent;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.NumericUpDown numericUpDown3;
        private System.Windows.Forms.NumericUpDown numericUpDown2;
        private System.Windows.Forms.NumericUpDown numericUpDown1;
        private System.Windows.Forms.VScrollBar vScrollBar2;
        private System.Windows.Forms.VScrollBar vScrollBar1;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Label nameLabel;
        private System.Windows.Forms.TextBox messageEntertextBox;
        private System.Windows.Forms.ListBox playersListBox;
        private System.ComponentModel.IContainer components = null;
        private System.Windows.Forms.TextBox playerNameTextBox;
        private System.Windows.Forms.RichTextBox messageTextBox;
        private System.Windows.Forms.CheckBox multiPlayerCheckBox;
        private System.Windows.Forms.ColumnHeader colHdrAction;
        private System.Windows.Forms.ColumnHeader colHdrControl;
        private System.Windows.Forms.ComboBox comboBox3;
        private System.Windows.Forms.GroupBox grpFirhter;
        private System.Windows.Forms.Label lblController;
        private System.Windows.Forms.Label lblPlayerName;
        private System.Windows.Forms.ComboBox cmbController;
        private System.Windows.Forms.Button btnRemovePlayer;
        private System.Windows.Forms.Button btnAddPlayer;
        private System.Windows.Forms.ComboBox cmbPlayerName;
        private System.Windows.Forms.TabControl tabControl;
        private System.Windows.Forms.TabPage tabTest;
        private System.Windows.Forms.TabPage tabInput;
        private System.Windows.Forms.ListView lviControls;
        private System.Timers.Timer inputDeviceConfigTimer;
        private System.Windows.Forms.OpenFileDialog openFileDialog1;
        private System.Windows.Forms.SaveFileDialog saveFileDialog1;
        private System.Windows.Forms.MenuItem menuFileOpenProfile;
        private System.Windows.Forms.MenuItem menuFileSaveAs;
        private System.Windows.Forms.Button doItButton;

        public MainForm()
        {
            //
            // Required for Windows Form Designer support
            //
            InitializeComponent();

            //
            // TODO: Add any constructor code after InitializeComponent call
            //
        }

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        protected override void Dispose( bool disposing )
        {
            if( disposing )
            {
                if (components != null) 
                {
                    components.Dispose();
                }
            }
            base.Dispose( disposing );
        }

        #region Windows Form Designer generated code
        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.levelTreeView = new System.Windows.Forms.TreeView();
            this.levelPictureBox = new System.Windows.Forms.PictureBox();
            this.mainMenu1 = new System.Windows.Forms.MainMenu(this.components);
            this.menuItem1 = new System.Windows.Forms.MenuItem();
            this.menuFileOpenProfile = new System.Windows.Forms.MenuItem();
            this.menuFileSaveCurrent = new System.Windows.Forms.MenuItem();
            this.menuFileSaveAs = new System.Windows.Forms.MenuItem();
            this.tabControl = new System.Windows.Forms.TabControl();
            this.tabLevel = new System.Windows.Forms.TabPage();
            this.tabInput = new System.Windows.Forms.TabPage();
            this.grpFirhter = new System.Windows.Forms.GroupBox();
            this.comboBox3 = new System.Windows.Forms.ComboBox();
            this.lviControls = new System.Windows.Forms.ListView();
            this.colHdrAction = new System.Windows.Forms.ColumnHeader();
            this.colHdrControl = new System.Windows.Forms.ColumnHeader();
            this.lblController = new System.Windows.Forms.Label();
            this.lblPlayerName = new System.Windows.Forms.Label();
            this.cmbController = new System.Windows.Forms.ComboBox();
            this.btnRemovePlayer = new System.Windows.Forms.Button();
            this.btnAddPlayer = new System.Windows.Forms.Button();
            this.cmbPlayerName = new System.Windows.Forms.ComboBox();
            this.tabTest = new System.Windows.Forms.TabPage();
            this.label3 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.numericUpDown3 = new System.Windows.Forms.NumericUpDown();
            this.numericUpDown2 = new System.Windows.Forms.NumericUpDown();
            this.numericUpDown1 = new System.Windows.Forms.NumericUpDown();
            this.vScrollBar2 = new System.Windows.Forms.VScrollBar();
            this.vScrollBar1 = new System.Windows.Forms.VScrollBar();
            this.panel1 = new System.Windows.Forms.Panel();
            this.doItButton = new System.Windows.Forms.Button();
            this.tabMultiPlayer = new System.Windows.Forms.TabPage();
            this.multiPlayerCheckBox = new System.Windows.Forms.CheckBox();
            this.messageTextBox = new System.Windows.Forms.RichTextBox();
            this.messageEntertextBox = new System.Windows.Forms.TextBox();
            this.playersListBox = new System.Windows.Forms.ListBox();
            this.nameLabel = new System.Windows.Forms.Label();
            this.playerNameTextBox = new System.Windows.Forms.TextBox();
            this.inputDeviceConfigTimer = new System.Timers.Timer();
            this.openFileDialog1 = new System.Windows.Forms.OpenFileDialog();
            this.saveFileDialog1 = new System.Windows.Forms.SaveFileDialog();
            ((System.ComponentModel.ISupportInitialize)(this.levelPictureBox)).BeginInit();
            this.tabControl.SuspendLayout();
            this.tabLevel.SuspendLayout();
            this.tabInput.SuspendLayout();
            this.grpFirhter.SuspendLayout();
            this.tabTest.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDown3)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDown2)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDown1)).BeginInit();
            this.tabMultiPlayer.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.inputDeviceConfigTimer)).BeginInit();
            this.SuspendLayout();
            // 
            // levelTreeView
            // 
            this.levelTreeView.Location = new System.Drawing.Point(24, 56);
            this.levelTreeView.Name = "levelTreeView";
            this.levelTreeView.Size = new System.Drawing.Size(256, 304);
            this.levelTreeView.TabIndex = 0;
            this.levelTreeView.AfterSelect += new System.Windows.Forms.TreeViewEventHandler(this.levelTreeView_AfterSelect);
            // 
            // levelPictureBox
            // 
            this.levelPictureBox.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.levelPictureBox.Location = new System.Drawing.Point(312, 16);
            this.levelPictureBox.Name = "levelPictureBox";
            this.levelPictureBox.Size = new System.Drawing.Size(336, 504);
            this.levelPictureBox.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.levelPictureBox.TabIndex = 1;
            this.levelPictureBox.TabStop = false;
            // 
            // mainMenu1
            // 
            this.mainMenu1.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
            this.menuItem1});
            // 
            // menuItem1
            // 
            this.menuItem1.Index = 0;
            this.menuItem1.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
            this.menuFileOpenProfile,
            this.menuFileSaveCurrent,
            this.menuFileSaveAs});
            this.menuItem1.Text = "&File";
            // 
            // menuFileOpenProfile
            // 
            this.menuFileOpenProfile.Index = 0;
            this.menuFileOpenProfile.Text = "Open Profile";
            this.menuFileOpenProfile.Click += new System.EventHandler(this.menuFileOpenProfile_Click);
            // 
            // menuFileSaveCurrent
            // 
            this.menuFileSaveCurrent.Index = 1;
            this.menuFileSaveCurrent.Text = "Save Current Profile";
            this.menuFileSaveCurrent.Click += new System.EventHandler(this.menuFileSaveCurrent_Click);
            // 
            // menuFileSaveAs
            // 
            this.menuFileSaveAs.Index = 2;
            this.menuFileSaveAs.Text = "Save Pofile As";
            this.menuFileSaveAs.Click += new System.EventHandler(this.menuFileSaveAs_Click);
            // 
            // tabControl
            // 
            this.tabControl.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.tabControl.Controls.Add(this.tabLevel);
            this.tabControl.Controls.Add(this.tabInput);
            this.tabControl.Controls.Add(this.tabTest);
            this.tabControl.Controls.Add(this.tabMultiPlayer);
            this.tabControl.Location = new System.Drawing.Point(8, 8);
            this.tabControl.Name = "tabControl";
            this.tabControl.SelectedIndex = 0;
            this.tabControl.Size = new System.Drawing.Size(688, 592);
            this.tabControl.TabIndex = 2;
            this.tabControl.SelectedIndexChanged += new System.EventHandler(this.tabControl1_SelectedIndexChanged);
            // 
            // tabLevel
            // 
            this.tabLevel.Controls.Add(this.levelPictureBox);
            this.tabLevel.Controls.Add(this.levelTreeView);
            this.tabLevel.Location = new System.Drawing.Point(4, 22);
            this.tabLevel.Name = "tabLevel";
            this.tabLevel.Size = new System.Drawing.Size(680, 566);
            this.tabLevel.TabIndex = 2;
            this.tabLevel.Text = "Level";
            // 
            // tabInput
            // 
            this.tabInput.Controls.Add(this.grpFirhter);
            this.tabInput.Controls.Add(this.lviControls);
            this.tabInput.Controls.Add(this.lblController);
            this.tabInput.Controls.Add(this.lblPlayerName);
            this.tabInput.Controls.Add(this.cmbController);
            this.tabInput.Controls.Add(this.btnRemovePlayer);
            this.tabInput.Controls.Add(this.btnAddPlayer);
            this.tabInput.Controls.Add(this.cmbPlayerName);
            this.tabInput.Location = new System.Drawing.Point(4, 22);
            this.tabInput.Name = "tabInput";
            this.tabInput.Size = new System.Drawing.Size(680, 566);
            this.tabInput.TabIndex = 4;
            this.tabInput.Text = "Input";
            // 
            // grpFirhter
            // 
            this.grpFirhter.Controls.Add(this.comboBox3);
            this.grpFirhter.Location = new System.Drawing.Point(288, 176);
            this.grpFirhter.Name = "grpFirhter";
            this.grpFirhter.Size = new System.Drawing.Size(376, 360);
            this.grpFirhter.TabIndex = 11;
            this.grpFirhter.TabStop = false;
            this.grpFirhter.Text = "Figther";
            // 
            // comboBox3
            // 
            this.comboBox3.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBox3.Location = new System.Drawing.Point(16, 24);
            this.comboBox3.Name = "comboBox3";
            this.comboBox3.Size = new System.Drawing.Size(336, 21);
            this.comboBox3.TabIndex = 9;
            // 
            // lviControls
            // 
            this.lviControls.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.colHdrAction,
            this.colHdrControl});
            this.lviControls.FullRowSelect = true;
            this.lviControls.Location = new System.Drawing.Point(24, 176);
            this.lviControls.Name = "lviControls";
            this.lviControls.Size = new System.Drawing.Size(248, 352);
            this.lviControls.TabIndex = 8;
            this.lviControls.UseCompatibleStateImageBehavior = false;
            this.lviControls.View = System.Windows.Forms.View.Details;
            this.lviControls.DoubleClick += new System.EventHandler(this.lviControls_DoubleClick);
            this.lviControls.Leave += new System.EventHandler(this.lviControls_Leave);
            // 
            // colHdrAction
            // 
            this.colHdrAction.Text = "Action";
            this.colHdrAction.Width = 86;
            // 
            // colHdrControl
            // 
            this.colHdrControl.Text = "Control";
            this.colHdrControl.Width = 151;
            // 
            // lblController
            // 
            this.lblController.Location = new System.Drawing.Point(64, 136);
            this.lblController.Name = "lblController";
            this.lblController.Size = new System.Drawing.Size(100, 23);
            this.lblController.TabIndex = 7;
            this.lblController.Text = "Controller";
            // 
            // lblPlayerName
            // 
            this.lblPlayerName.Location = new System.Drawing.Point(24, 24);
            this.lblPlayerName.Name = "lblPlayerName";
            this.lblPlayerName.Size = new System.Drawing.Size(100, 23);
            this.lblPlayerName.TabIndex = 6;
            this.lblPlayerName.Text = "Player";
            // 
            // cmbController
            // 
            this.cmbController.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbController.Location = new System.Drawing.Point(176, 136);
            this.cmbController.Name = "cmbController";
            this.cmbController.Size = new System.Drawing.Size(200, 21);
            this.cmbController.TabIndex = 5;
            this.cmbController.SelectedIndexChanged += new System.EventHandler(this.cmbController_SelectedIndexChanged);
            // 
            // btnRemovePlayer
            // 
            this.btnRemovePlayer.Location = new System.Drawing.Point(352, 24);
            this.btnRemovePlayer.Name = "btnRemovePlayer";
            this.btnRemovePlayer.Size = new System.Drawing.Size(104, 23);
            this.btnRemovePlayer.TabIndex = 4;
            this.btnRemovePlayer.Text = "Remove Player";
            this.btnRemovePlayer.Click += new System.EventHandler(this.btnRemovePlayer_Click);
            // 
            // btnAddPlayer
            // 
            this.btnAddPlayer.Location = new System.Drawing.Point(464, 24);
            this.btnAddPlayer.Name = "btnAddPlayer";
            this.btnAddPlayer.Size = new System.Drawing.Size(104, 23);
            this.btnAddPlayer.TabIndex = 3;
            this.btnAddPlayer.Text = "Add Player";
            this.btnAddPlayer.Click += new System.EventHandler(this.btnAddPlayer_Click);
            // 
            // cmbPlayerName
            // 
            this.cmbPlayerName.Location = new System.Drawing.Point(152, 24);
            this.cmbPlayerName.Name = "cmbPlayerName";
            this.cmbPlayerName.Size = new System.Drawing.Size(192, 21);
            this.cmbPlayerName.TabIndex = 0;
            this.cmbPlayerName.SelectedIndexChanged += new System.EventHandler(this.cmbPlayerName_SelectedIndexChanged);
            this.cmbPlayerName.KeyUp += new System.Windows.Forms.KeyEventHandler(this.cmbPlayerName_KeyUp);
            this.cmbPlayerName.Validated += new System.EventHandler(this.cmbPlayerName_Validated);
            this.cmbPlayerName.DropDown += new System.EventHandler(this.cmbPlayerName_DropDown);
            // 
            // tabTest
            // 
            this.tabTest.Controls.Add(this.label3);
            this.tabTest.Controls.Add(this.label2);
            this.tabTest.Controls.Add(this.label1);
            this.tabTest.Controls.Add(this.numericUpDown3);
            this.tabTest.Controls.Add(this.numericUpDown2);
            this.tabTest.Controls.Add(this.numericUpDown1);
            this.tabTest.Controls.Add(this.vScrollBar2);
            this.tabTest.Controls.Add(this.vScrollBar1);
            this.tabTest.Controls.Add(this.panel1);
            this.tabTest.Controls.Add(this.doItButton);
            this.tabTest.Location = new System.Drawing.Point(4, 22);
            this.tabTest.Name = "tabTest";
            this.tabTest.Size = new System.Drawing.Size(680, 566);
            this.tabTest.TabIndex = 3;
            this.tabTest.Text = "Test";
            // 
            // label3
            // 
            this.label3.Location = new System.Drawing.Point(8, 176);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(8, 20);
            this.label3.TabIndex = 10;
            this.label3.Text = "t";
            this.label3.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // label2
            // 
            this.label2.Location = new System.Drawing.Point(8, 144);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(8, 20);
            this.label2.TabIndex = 9;
            this.label2.Text = "z";
            this.label2.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // label1
            // 
            this.label1.Location = new System.Drawing.Point(8, 112);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(8, 20);
            this.label1.TabIndex = 8;
            this.label1.Text = "g";
            this.label1.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // numericUpDown3
            // 
            this.numericUpDown3.DecimalPlaces = 4;
            this.numericUpDown3.Increment = new decimal(new int[] {
            25,
            0,
            0,
            0});
            this.numericUpDown3.Location = new System.Drawing.Point(32, 176);
            this.numericUpDown3.Maximum = new decimal(new int[] {
            1000,
            0,
            0,
            0});
            this.numericUpDown3.Minimum = new decimal(new int[] {
            1000,
            0,
            0,
            -2147483648});
            this.numericUpDown3.Name = "numericUpDown3";
            this.numericUpDown3.Size = new System.Drawing.Size(72, 20);
            this.numericUpDown3.TabIndex = 7;
            this.numericUpDown3.Value = new decimal(new int[] {
            200,
            0,
            0,
            0});
            // 
            // numericUpDown2
            // 
            this.numericUpDown2.DecimalPlaces = 4;
            this.numericUpDown2.Increment = new decimal(new int[] {
            1,
            0,
            0,
            65536});
            this.numericUpDown2.Location = new System.Drawing.Point(32, 144);
            this.numericUpDown2.Maximum = new decimal(new int[] {
            1000,
            0,
            0,
            0});
            this.numericUpDown2.Minimum = new decimal(new int[] {
            1000,
            0,
            0,
            -2147483648});
            this.numericUpDown2.Name = "numericUpDown2";
            this.numericUpDown2.Size = new System.Drawing.Size(72, 20);
            this.numericUpDown2.TabIndex = 6;
            this.numericUpDown2.Value = new decimal(new int[] {
            7,
            0,
            0,
            65536});
            // 
            // numericUpDown1
            // 
            this.numericUpDown1.DecimalPlaces = 4;
            this.numericUpDown1.Increment = new decimal(new int[] {
            10,
            0,
            0,
            0});
            this.numericUpDown1.Location = new System.Drawing.Point(32, 112);
            this.numericUpDown1.Maximum = new decimal(new int[] {
            1000,
            0,
            0,
            0});
            this.numericUpDown1.Minimum = new decimal(new int[] {
            1000,
            0,
            0,
            -2147483648});
            this.numericUpDown1.Name = "numericUpDown1";
            this.numericUpDown1.Size = new System.Drawing.Size(72, 20);
            this.numericUpDown1.TabIndex = 5;
            this.numericUpDown1.Value = new decimal(new int[] {
            50,
            0,
            0,
            0});
            // 
            // vScrollBar2
            // 
            this.vScrollBar2.LargeChange = 1;
            this.vScrollBar2.Location = new System.Drawing.Point(48, 296);
            this.vScrollBar2.Maximum = 50;
            this.vScrollBar2.Minimum = -50;
            this.vScrollBar2.Name = "vScrollBar2";
            this.vScrollBar2.Size = new System.Drawing.Size(16, 216);
            this.vScrollBar2.TabIndex = 3;
            // 
            // vScrollBar1
            // 
            this.vScrollBar1.LargeChange = 1;
            this.vScrollBar1.Location = new System.Drawing.Point(16, 296);
            this.vScrollBar1.Maximum = 50;
            this.vScrollBar1.Minimum = -50;
            this.vScrollBar1.Name = "vScrollBar1";
            this.vScrollBar1.Size = new System.Drawing.Size(16, 216);
            this.vScrollBar1.TabIndex = 2;
            // 
            // panel1
            // 
            this.panel1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.panel1.Location = new System.Drawing.Point(112, 16);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(552, 536);
            this.panel1.TabIndex = 1;
            // 
            // doItButton
            // 
            this.doItButton.Location = new System.Drawing.Point(16, 40);
            this.doItButton.Name = "doItButton";
            this.doItButton.Size = new System.Drawing.Size(75, 23);
            this.doItButton.TabIndex = 0;
            this.doItButton.Text = "Do It";
            this.doItButton.Click += new System.EventHandler(this.doItButton_Click);
            // 
            // tabMultiPlayer
            // 
            this.tabMultiPlayer.Controls.Add(this.multiPlayerCheckBox);
            this.tabMultiPlayer.Controls.Add(this.messageTextBox);
            this.tabMultiPlayer.Controls.Add(this.messageEntertextBox);
            this.tabMultiPlayer.Controls.Add(this.playersListBox);
            this.tabMultiPlayer.Controls.Add(this.nameLabel);
            this.tabMultiPlayer.Controls.Add(this.playerNameTextBox);
            this.tabMultiPlayer.Location = new System.Drawing.Point(4, 22);
            this.tabMultiPlayer.Name = "tabMultiPlayer";
            this.tabMultiPlayer.Size = new System.Drawing.Size(680, 566);
            this.tabMultiPlayer.TabIndex = 1;
            this.tabMultiPlayer.Text = "Multiplayer";
            // 
            // multiPlayerCheckBox
            // 
            this.multiPlayerCheckBox.Location = new System.Drawing.Point(32, 16);
            this.multiPlayerCheckBox.Name = "multiPlayerCheckBox";
            this.multiPlayerCheckBox.Size = new System.Drawing.Size(104, 24);
            this.multiPlayerCheckBox.TabIndex = 6;
            this.multiPlayerCheckBox.Text = "MultiPlayer";
            this.multiPlayerCheckBox.CheckedChanged += new System.EventHandler(this.multiPlayerCheckBox_CheckedChanged);
            // 
            // messageTextBox
            // 
            this.messageTextBox.Font = new System.Drawing.Font("Courier New", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.messageTextBox.Location = new System.Drawing.Point(48, 112);
            this.messageTextBox.Name = "messageTextBox";
            this.messageTextBox.ReadOnly = true;
            this.messageTextBox.Size = new System.Drawing.Size(336, 224);
            this.messageTextBox.TabIndex = 5;
            this.messageTextBox.Text = "";
            // 
            // messageEntertextBox
            // 
            this.messageEntertextBox.Location = new System.Drawing.Point(48, 344);
            this.messageEntertextBox.Name = "messageEntertextBox";
            this.messageEntertextBox.Size = new System.Drawing.Size(336, 20);
            this.messageEntertextBox.TabIndex = 4;
            this.messageEntertextBox.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.messageEntertextBox_KeyPress);
            // 
            // playersListBox
            // 
            this.playersListBox.Location = new System.Drawing.Point(464, 112);
            this.playersListBox.Name = "playersListBox";
            this.playersListBox.Size = new System.Drawing.Size(232, 251);
            this.playersListBox.TabIndex = 2;
            // 
            // nameLabel
            // 
            this.nameLabel.Location = new System.Drawing.Point(48, 80);
            this.nameLabel.Name = "nameLabel";
            this.nameLabel.Size = new System.Drawing.Size(100, 20);
            this.nameLabel.TabIndex = 1;
            this.nameLabel.Text = "Name";
            this.nameLabel.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // playerNameTextBox
            // 
            this.playerNameTextBox.Location = new System.Drawing.Point(160, 80);
            this.playerNameTextBox.Name = "playerNameTextBox";
            this.playerNameTextBox.Size = new System.Drawing.Size(224, 20);
            this.playerNameTextBox.TabIndex = 0;
            this.playerNameTextBox.Leave += new System.EventHandler(this.playerNameTextBox_Leave);
            this.playerNameTextBox.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.playerNameTextBox_KeyPress);
            // 
            // inputDeviceConfigTimer
            // 
            this.inputDeviceConfigTimer.SynchronizingObject = this;
            this.inputDeviceConfigTimer.Elapsed += new System.Timers.ElapsedEventHandler(this.inputDeviceConfigTimer_Elapsed);
            // 
            // MainForm
            // 
            this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
            this.ClientSize = new System.Drawing.Size(704, 609);
            this.Controls.Add(this.tabControl);
            this.Menu = this.mainMenu1;
            this.Name = "MainForm";
            this.Text = "Turbo Gravity Lemming Raketty Force";
            this.Load += new System.EventHandler(this.Form1_Load);
            ((System.ComponentModel.ISupportInitialize)(this.levelPictureBox)).EndInit();
            this.tabControl.ResumeLayout(false);
            this.tabLevel.ResumeLayout(false);
            this.tabInput.ResumeLayout(false);
            this.grpFirhter.ResumeLayout(false);
            this.tabTest.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDown3)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDown2)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDown1)).EndInit();
            this.tabMultiPlayer.ResumeLayout(false);
            this.tabMultiPlayer.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.inputDeviceConfigTimer)).EndInit();
            this.ResumeLayout(false);

        }
        #endregion

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main() 
        {
            MainForm form = new MainForm();
            form.Show();
            while(form.Created)
            {
              //  Application.Run(new MainForm());
                Application.DoEvents();
                form.Render();
            }
        }

        private void Form1_Load(object sender, System.EventArgs e)
        {
            DirectorySettings.Check();
            Settings.Current = Settings.LoadCurrentProfile();
            
            playerNameTextBox.Text = Settings.Current.Players[0].Name;
            
			//networking = Networking.GetInstance();
			//networking.SetPlayerInformation(playerNameTextBox.Text);
			//networking.PlayerChanged += new tglrf.Networking.PlayerChangedEventHandler(networking_PlayerChanged);
			//networking.MessageReceived += new tglrf.Networking.MessageReceivedEventHandler(networking_MessageReceived);
            //networking.Host("hoi");

			FillLevelTreeView();

            SoundHandler.Initialize(this);
        }

        private void FillLevelTreeView()
        {
            string gf2Dir = Path.Combine(DirectorySettings.MediaDir, @"level/gf2/levels");
            DirectoryInfo dirInfo = new DirectoryInfo(gf2Dir);
            if(dirInfo.Exists)
            {
                levelTreeView.Nodes.Clear();
                Hashtable types = new Hashtable();
                TreeNode selectNode = null;

                foreach(FileInfo fileInfo in dirInfo.GetFiles("*.GFB"))
                {
                    GravitiForceLevel gfl = GravitiForceLevel.ReadGravitiForceLevelFile(fileInfo.Name);
                    if(gfl.type.Bitmap != "ZIFF")
                    {
                        TreeNode typeNode = (TreeNode)types[gfl.type.Bitmap];
                        if(typeNode == null)
                        {
                            typeNode = new TreeNode(gfl.type.Name);
                            types[gfl.type.Bitmap] = typeNode;
                            levelTreeView.Nodes.Add(typeNode);
                        }
                        TreeNode levelNode = new TreeNode(gfl.Name);
                        levelNode.Tag = gfl;
                        typeNode.Nodes.Add(levelNode);
                        if(gfl.FileName == Settings.Current.FileName)
                        {
                            selectNode = levelNode;
                        }
                    }
                }
                if(selectNode != null)
                {
                    levelTreeView.SelectedNode = selectNode;
                    levelTreeView.Select();
                }
            }
        }

        public bool ThumbnailCallback()
        {
            return false;
        }



        private void levelTreeView_AfterSelect(object sender, System.Windows.Forms.TreeViewEventArgs e)
        {
            GravitiForceLevel gfl = null;
            TreeNode typeNode = levelTreeView.SelectedNode;
            if(typeNode != null)
            {
                gfl = typeNode.Tag as GravitiForceLevel;
            }
            if(gfl != null)
            {
                levelPictureBox.Image = gfl.GetBitmap();
                Settings.Current.FileName = gfl.FileName;
            }
        }












        private Device device;
        public static LevelBackgroundGF levelBackground;
        private ObjectShip[] playerShips;
        private BulletBuffer bullerBuffer;
        private ShipBase[] shipBase;
        private GravitiForceLevel gfl;
        int  lastTime;
        private bool deviceLost = true;

        public bool InitializeGraphics()
        {
            try
            {
                // Now let's setup our D3D stuff
                PresentParameters presentParams = new PresentParameters();
                presentParams.IsWindowed = true;
                presentParams.SwapEffect = SwapEffect.Discard;
                presentParams.EnableAutoDepthStencil = true;
                presentParams.AutoDepthStencilFormat = DepthFormat.D16;
                device = new Device(0, DeviceType.Hardware, panel1.Handle, CreateFlags.SoftwareVertexProcessing, presentParams);

                device.DeviceReset += new System.EventHandler(this.OnResetDevice);
                this.OnResetDevice(device, null);
            }
            catch (DirectXException ex)
            { 
                ex.ToString();
                return false; 
            }
            return true;
        }

        public void OnResetDevice(object sender, EventArgs e)
        {
            Device dev = (Device)sender;
            gfl = null;

            playerShips = new ObjectShip[Settings.Current.Players.Length];

            if((levelTreeView.SelectedNode != null) && (levelTreeView.SelectedNode.Tag is GravitiForceLevel))
            {
                gfl = (GravitiForceLevel)levelTreeView.SelectedNode.Tag;
                levelBackground = LevelBackgroundGF.CreateLevelBackground(dev, gfl);

                shipBase = new ShipBase[2];
                shipBase[0] = new ShipBase(new Vector3((float)(gfl.playerBase[0].X - 104), (float)(997f - gfl.playerBase[0].Y), 0f));
                shipBase[1] = new ShipBase(new Vector3((float)(gfl.playerBase[1].X - 104), (float)(997f - gfl.playerBase[1].Y), 0f));

                for(int i = 0; i < playerShips.Length; i++)
                {
                    playerShips[i] = ObjectShip.CreateShip(dev);

                    playerShips[i].Position = shipBase[i].Position;
                }
                bullerBuffer = new BulletBuffer(dev);
            }
            else
            {
                //levelBackground = LevelBackgroundTest.CreateLevelBackground(dev);
            }
            lastTime = Environment.TickCount;

            deviceLost = false;
        }

        void SetupMatrices(Vector3 cameraTarget)
        {
            device.Transform.Projection = Matrix.PerspectiveFieldOfViewLeftHanded(
                (float)(Math.PI / 4),
                (float)((double)device.Viewport.Width /  device.Viewport.Height),
                1.0f, 10000.0f );
            float cameraDistance = 250f + (vScrollBar1.Value * 8f);

            float offset = cameraDistance / device.Transform.Projection.M11;
            if(cameraTarget.X < offset)
            {
                cameraTarget.X = offset;
            }
            if(cameraTarget.X > (21 * 16 - offset))
            {
                cameraTarget.X = 21 * 16 - offset;
            }
            offset = cameraDistance / device.Transform.Projection.M22;
            if(cameraTarget.Y < offset)
            {
                cameraTarget.Y = offset;
            }
            if(cameraTarget.Y > (63 * 16 - offset))
            {
                cameraTarget.Y = 63 * 16 - offset;
            }

            Vector3 cameraPosition = cameraTarget + new Vector3(0f,0f,-cameraDistance);
            //            Matrix tmp = Matrix.RotationZ(player1Ship.Rotation.Z);
            //            cameraPosition.X -= vScrollBar2.Value * 20f * tmp.M12;
            //            cameraPosition.Y += vScrollBar2.Value * 20f * tmp.M11;
            // Set up our view matrix. A view matrix can be defined given an eye point,
            // a point to lookat, and a direction for which way is up. Here, we set the
            // eye five units back along the z-axis and up three units, look at the 
            // origin, and define "up" to be in the y-direction.
            
            device.Transform.View = Matrix.LookAtLeftHanded(
                cameraPosition, 
                cameraTarget,
                new Vector3(0f, 1f, 0f ) );
            //                new Vector3(- tmp.M12, tmp.M11, 0.0f ) );



            // For the projection matrix, we set up a perspective transform (which
            // transforms geometry from 3D view space to 2D viewport space, with
            // a perspective divide making objects smaller in the distance). To build
            // a perpsective transform, we need the field of view (1/4 pi is common),
            // the aspect ratio, and the near and far clipping planes (which define at
            // what distances geometry should be no longer be rendered).
        }


        private void SetUpLights()
        {
            System.Drawing.Color col = System.Drawing.Color.White;
            //Set up a material. The material here just has the diffuse and ambient
            //colors set to yellow. Note that only one material can be used at a time.
            Direct3D.Material mtrl = new Direct3D.Material();
            mtrl.DiffuseColor = ColorValue.FromColor(col);
            mtrl.AmbientColor = ColorValue.FromColor(col);
            device.Material = mtrl;
			
            //Set up a white, directional light, with an oscillating direction.
            //Note that many lights may be active at a time (but each one slows down
            //the rendering of our scene). However, here we are just using one. Also,
            //we need to set the D3DRS_LIGHTING renderstate to enable lighting
    
            device.Lights[0].LightType = (LightType)0;
            device.Lights[0].Diffuse = System.Drawing.Color.White;
            device.Lights[0].Direction = new Vector3(
                (float)(Math.Cos(Environment.TickCount/350.0)),
                -1.0f,
                (float)(Math.Sin(Environment.TickCount / 350.0)));
            device.Lights[0].Enabled = true;//turn it on
            //Finally, turn on some ambient light.
            //Ambient light is light that scatters and lights all objects evenly
            device.RenderState.Ambient = System.Drawing.Color.FromArgb(0x505050);
            //device.Lights[0].Commit();//let d3d know about the light
        }



        private void Render()
        {
            if((device == null) || deviceLost) 
                return;

            bool inputOk = InputHandler.HandleInput();

            TestSettings.Value1 = (double)numericUpDown1.Value;
            TestSettings.Value2 = (double)numericUpDown2.Value;
            TestSettings.Value3 = (double)numericUpDown3.Value;

            int currentTime = Environment.TickCount;
            double timeElapsed = (currentTime - lastTime) / 1000.0;
            if(inputOk)
            {
                for(int i = 0; i < InputHandler.Player.Length; i++)
                {
                    playerShips[i].HandleController(InputHandler.Player[i], timeElapsed);
                }
            }
            for(int i = 0; i < shipBase.Length; i++)
            {
                for(int j = 0; j < playerShips.Length; j++)
                {
                    shipBase[i].Interact(playerShips[j]);
                }
            }
            for(int i = 0; i < playerShips.Length - 1; i++)
            {
                for(int j = 1; j < playerShips.Length; j++)
                {
                    playerShips[i].Bots(playerShips[j]);
                }
            }
            lastTime = currentTime;

            Viewport original;
            try
            {
                 original = device.Viewport;            
            }
            catch
            {
                deviceLost = true;
                return;
            }
            Viewport newView = original;
            newView.Width = newView.Width / InputHandler.Player.Length - (0 * (InputHandler.Player.Length - 1));

            for(int player = 0; player < InputHandler.Player.Length; player++)
            {
                //Begin the scene
                device.BeginScene();
                device.Viewport = newView;

                //Clear the backbuffer to a blue color 
                device.Clear(ClearFlags.ZBuffer | ClearFlags.Target, System.Drawing.Color.Blue, 1.0f, 0);
                // background
                device.RenderState.ZBufferEnable = false;

                SetupMatrices(playerShips[player].Position);

                levelBackground.Render(device);

                SetUpLights();
                for(int i = 0; i < InputHandler.Player.Length; i++)
                {
                    playerShips[i].Render(device, vScrollBar1.Value, vScrollBar2.Value);
                }

                Direct3D.Font font = new Direct3D.Font(device, new System.Drawing.Font("Arial", 10));
                font.DrawString(null,
                    string.Format("{0:00.00} {1:00.00}", playerShips[player].Position.X, playerShips[player].Position.Y),
                    new Rectangle(newView.X + 20, 20, 200, 30), DrawStringFormat.None, Color.Yellow);
                font.Dispose();


                newView.X += newView.Width + 0;

                for(int i = 0; i < InputHandler.Player.Length; i++)
                {
                    bullerBuffer.Render(device,  playerShips[i]);
                }


                //End the scene
                device.EndScene();
            }

            device.Present();
            device.Viewport = original;
        }


        private void tabControl1_SelectedIndexChanged(object sender, System.EventArgs e)
        {
            InputHandler.Free();
            if(tabControl.SelectedTab == tabTest)
            {
                InputHandler.Init(this);
                InitializeGraphics();  
            }
            else if(tabControl.SelectedTab == tabInput)
            {
                tabInput_Selected(sender, e);
            }
        }

        private void menuFileOpenProfile_Click(object sender, System.EventArgs e)
        {
            if(openFileDialog1.ShowDialog(this) == DialogResult.OK)
            {
                using(Stream fs = openFileDialog1.OpenFile())
                {
                    Settings.Current = Settings.Load(fs);
                    saveFileDialog1.FileName = openFileDialog1.FileName;
                    FillLevelTreeView();
                }
            }
        }

        private void menuFileSaveCurrent_Click(object sender, System.EventArgs e)
        {
            using(FileStream fs = new FileStream(Path.Combine(DirectorySettings.ConfigDir, Settings.CurrentProfileName),
                      FileMode.Create, FileAccess.Write))
            {
                Settings.Current.Save(fs);
            }
        }

        private void menuFileSaveAs_Click(object sender, System.EventArgs e)
        {
            if(saveFileDialog1.ShowDialog(this) == DialogResult.OK)
            {
                using(Stream fs = saveFileDialog1.OpenFile())
                {
                    Settings.Current.Save(fs);
                }
            }
        }

        private void networking_PlayerChanged(object sender)
        {
            lock(this)
            {
				//DirectPlay.PlayerInformation[] players = networking.GetPlayerList();
				//string[] playersNames = new string[players.Length];
				//for(int i = 0; i < players.Length; i++)
				//{
				//    playersNames[i] = players[i].Name;
				//    //                    if(players[i].Host)
				//    //                    {
				//    //                        playersNames[i] += " [host]";
				//    //                    }
				//}
				//Array.Sort(playersNames);
				//playersListBox.DataSource = playersNames;
            }
        }

		//private void networking_MessageReceived(DirectPlay.PlayerInformation player, string message)
		//{
		//    messageTextBox.Text += string.Format("{0:-14} : {1}\n", player.Name, message);
		//    messageTextBox.SelectionStart = messageTextBox.Text.Length;
		//    messageTextBox.ScrollToCaret();
		//}


        private void messageEntertextBox_KeyPress(object sender, System.Windows.Forms.KeyPressEventArgs e)
        {
            if(e.KeyChar == '\r')
            {
                if(messageEntertextBox.Text.Trim().Length > 0)
                {
					//networking.SendChatMessage(messageEntertextBox.Text);
					//messageEntertextBox.Text = "";
                }
            }
        }

        private void playerNameTextBox_KeyPress(object sender, System.Windows.Forms.KeyPressEventArgs e)
        {
            if(e.KeyChar == '\r')
            {
                playerNameTextBox_Leave(sender, e);
            }
        }
        private void playerNameTextBox_Leave(object sender, System.EventArgs e)
        {
            if(playerNameTextBox.Text.Trim().Length == 0)
            {
                playerNameTextBox.Text = "Player X";
            }
			//networking.SetPlayerInformation(playerNameTextBox.Text);
            Settings.Current.Players[0].Name = playerNameTextBox.Text;
        }

        private void multiPlayerCheckBox_CheckedChanged(object sender, System.EventArgs e)
        {
            if(multiPlayerCheckBox.Enabled && multiPlayerCheckBox.Checked)
            {
				//if(networking.DoWizzard())
				//{
				//    multiPlayerCheckBox.Enabled = false;
				//    multiPlayerCheckBox.Checked = true;
				//}
				//else
				//{
				//    multiPlayerCheckBox.Enabled = true;
				//    multiPlayerCheckBox.Checked = false;
				//}
            }
        }

        #region DirectInput configure

        private DirectInput.DeviceInstance[]    inputAvailableDevices;
        private int                             inputSelectedPlayerIndex;
        private ListViewItem                    inputSelectedListViewItem;
        private DirectInput.Device              inputDevice;
        private string                          inputPreviousSelectedControl;

        private void tabInput_Selected(object sender, System.EventArgs e)
        {
            inputSelectedPlayerIndex = 0;

            ArrayList tmp = new ArrayList();

            foreach(DirectInput.DeviceInstance instance in DirectInput.Manager.GetDevices(
                DirectInput.DeviceClass.GameControl,
                DirectInput.EnumDevicesFlags.AttachedOnly))
            {
                tmp.Add(instance);
            }
            foreach(DirectInput.DeviceInstance instance in DirectInput.Manager.GetDevices(
                DirectInput.DeviceClass.Keyboard,
                DirectInput.EnumDevicesFlags.AttachedOnly))
            {
                tmp.Add(instance);
            }
            inputAvailableDevices = (DirectInput.DeviceInstance[])tmp.ToArray(typeof(DirectInput.DeviceInstance));

            cmbController.Items.Clear();
            foreach(Microsoft.DirectX.DirectInput.DeviceInstance deviceInstance in inputAvailableDevices)
            {
                cmbController.Items.Add(deviceInstance.InstanceName);
            }
            
            cmbPlayerName.Items.Clear();
            foreach(PlayerConfig player in Settings.Current.Players)
            {
                cmbPlayerName.Items.Add(player.Name);
            }
            cmbPlayerName.SelectedIndex = 0;

        }

        private void cmbPlayerName_SelectedIndexChanged(object sender, System.EventArgs e)
        {
            inputSelectedPlayerIndex = cmbPlayerName.SelectedIndex;

            btnAddPlayer.Enabled = (Settings.Current.Players.Length < 2) && 
                (inputAvailableDevices.Length > Settings.Current.Players.Length);
            btnRemovePlayer.Enabled = Settings.Current.Players.Length > 1;

            for(int i = 0; i < inputAvailableDevices.Length; i++)
            {
                if(Settings.Current.Players[inputSelectedPlayerIndex].Controller == inputAvailableDevices[i].InstanceName)  // Should use GUID??
                {
                    cmbController.SelectedIndex = i;
                    break;
                }
            }
            DisplayControls();
        }

        private void btnAddPlayer_Click(object sender, System.EventArgs e)
        {
            PlayerConfig[] config = new PlayerConfig[Settings.Current.Players.Length + 1];
            Array.Copy(Settings.Current.Players, config, Settings.Current.Players.Length);
            config[Settings.Current.Players.Length] = new PlayerConfig();
            Settings.Current.Players = config;
            cmbPlayerName.Items.Add(config[config.Length - 1].Name);

            cmbPlayerName.SelectedIndex = config.Length - 1;
        }

        private void btnRemovePlayer_Click(object sender, System.EventArgs e)
        {
            if(cmbPlayerName.SelectedIndex >= 0)
            {
                ArrayList tmp = new ArrayList(Settings.Current.Players);
                tmp.RemoveAt(cmbPlayerName.SelectedIndex);
                cmbPlayerName.Items.RemoveAt(cmbPlayerName.SelectedIndex);
                Settings.Current.Players = (PlayerConfig[])tmp.ToArray(typeof(PlayerConfig));
                cmbPlayerName.SelectedIndex = 0;
            }
        }

        private void cmbPlayerName_Validated(object sender, System.EventArgs e)
        {
            Settings.Current.Players[inputSelectedPlayerIndex].Name = cmbPlayerName.Text;
            cmbPlayerName.Items[inputSelectedPlayerIndex] = cmbPlayerName.Text;
        }

        private void cmbPlayerName_KeyUp(object sender, System.Windows.Forms.KeyEventArgs e)
        {
            if(e.KeyCode == System.Windows.Forms.Keys.Enter)
            {
                cmbPlayerName_Validated(sender, e);
            }
        }

        private void cmbPlayerName_DropDown(object sender, System.EventArgs e)
        {
            cmbPlayerName_Validated(sender, e);
        }

        private void cmbController_SelectedIndexChanged(object sender, System.EventArgs e)
        {
            string playerPreviousController = Settings.Current.Players[inputSelectedPlayerIndex].Controller;
            string selectedController = inputAvailableDevices[cmbController.SelectedIndex].InstanceName;
            if(selectedController != playerPreviousController)
            {
                int otherPlayerWithSelectedController = -1;
                for(int i = 0; i < Settings.Current.Players.Length; i++)
                {
                    if(i != inputSelectedPlayerIndex)
                    {
                        if(Settings.Current.Players[i].Controller == selectedController)
                        {
                            otherPlayerWithSelectedController = i;
                            break;
                        }
                    }
                }
                if(otherPlayerWithSelectedController >= 0)
                {
                    Controls tmpControls = Settings.Current.Players[otherPlayerWithSelectedController].Controls;

                    Settings.Current.Players[otherPlayerWithSelectedController].Controller = playerPreviousController;
                    Settings.Current.Players[otherPlayerWithSelectedController].Controls   = Settings.Current.Players[inputSelectedPlayerIndex].Controls;

                    Settings.Current.Players[inputSelectedPlayerIndex].Controls = tmpControls;
                }
                Settings.Current.Players[inputSelectedPlayerIndex].Controller = selectedController;

                DisplayControls();
            }
        }

        private void DisplayControls()
        {
            if((Settings.Current.Players == null) ||
                (Settings.Current.Players.Length <= inputSelectedPlayerIndex) ||
                (cmbController.SelectedIndex < 0))
            {
                return;
            }
            Controls controls = Settings.Current.Players[inputSelectedPlayerIndex].Controls;
            Microsoft.DirectX.DirectInput.DeviceInstance controller = inputAvailableDevices[cmbController.SelectedIndex];
            DirectInput.Device device = new DirectInput.Device(controller.Instance);

            //            int pop = device.Properties.GetDeadZone(DirectInput.ParameterHow.ById, 0);
            //            pop = device.Properties.GetDeadZone(DirectInput.ParameterHow.ById, 1);
            //            pop = device.Properties.GetDeadZone(DirectInput.ParameterHow.ById, 2);
            //            pop = device.Properties.GetDeadZone(DirectInput.ParameterHow.ById, 3);
            bool analog = device.Capabilities.NumberAxes > 0;

            lviControls.Items.Clear();
            foreach(FieldInfo field in controls.GetType().GetFields())
            {
                bool analogField = field.GetCustomAttributes(typeof(AnalogControlAttribute), true).Length > 0;
                if(analog || !analogField)
                {
                    ListViewItem item = new ListViewItem(new string[] {field.Name, (string)field.GetValue(controls)});
                    item.Tag = field;
                    lviControls.Items.Add(item);
                }
            }
            device.Dispose();
        }


        private void lviControls_DoubleClick(object sender, System.EventArgs e)
        {
            if((lviControls.SelectedItems.Count > 0) &&
                (lviControls.Focused))
            {
                if(inputSelectedListViewItem != null)
                {
                    lviControls_Leave(sender, e);
                }
                inputPreviousSelectedControl = null;
                Microsoft.DirectX.DirectInput.DeviceInstance controller = inputAvailableDevices[cmbController.SelectedIndex];
                inputDevice = new DirectInput.Device(controller.Instance);
                if(inputDevice.Capabilities.DeviceType == DirectInput.DeviceType.Joystick)
                {
                    inputDevice.SetDataFormat(DirectInput.DeviceDataFormat.Joystick);
                    foreach (DirectInput.ObjectInstance d in inputDevice.Objects)
                    {
                        // For axes that are returned, set the DIPROP_RANGE property for the
                        // enumerated axis in order to scale min/max values.

                        if ((0 != (d.Type & (int)DirectInput.ObjectTypeFlags.Axis)))
                        {
                            // Set the range for the axis.
                            inputDevice.Properties.SetRange(DirectInput.ParameterHow.ById, d.Type, new DirectInput.InputRange(-1000, +1000));
                        }
                    }
                }
                else
                {
                    inputDevice.SetDataFormat(DirectInput.DeviceDataFormat.Keyboard);
                }
                inputDevice.SetCooperativeLevel(this.Handle, DirectInput.CooperativeLevelFlags.Foreground | DirectInput.CooperativeLevelFlags.NonExclusive);


                inputSelectedListViewItem = lviControls.SelectedItems[0];
                lviControls.SelectedItems.Clear();
                inputSelectedListViewItem.BackColor = Color.Red;
                inputDeviceConfigTimer.Start();
            }
        }

        private void lviControls_Leave(object sender, System.EventArgs e)
        {
            if(inputSelectedListViewItem != null)
            {
                inputDevice.Dispose();
                inputDevice = null;

                inputSelectedListViewItem.BackColor = Color.White;
                inputSelectedListViewItem = null;
                inputDeviceConfigTimer.Stop();
            }
        }

        private void inputDeviceConfigTimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            if(inputDevice == null)
            {
                return;
            }
            
            try
            {
                inputDevice.Poll();
            }
            catch(DirectXException ex)
            {
                if ((ex is DirectInput.NotAcquiredException) || (ex is DirectInput.InputLostException))
                {
                    try
                    {
                        inputDevice.Acquire();
                    }
					catch (DirectXException)
                    {
                        return;
                    }
                }
            }

            FieldInfo field = inputSelectedListViewItem.Tag as FieldInfo;
            if(field != null)
            {
                bool analogField = field.GetCustomAttributes(typeof(AnalogControlAttribute), true).Length > 0;

                string selectedControl = null;

                if(inputDevice.Capabilities.DeviceType == DirectInput.DeviceType.Joystick)
                {
                    DirectInput.JoystickState state;

                    try
                    {
                        state = inputDevice.CurrentJoystickState;
                    }
                    catch(DirectXException)
                    {
                        return;
                    }

                    if(analogField)
                    {
                        foreach(PropertyInfo prop in state.GetType().GetProperties())
                        {
                            if(prop.PropertyType == typeof(int))
                            {
                                int propValue = (int)prop.GetValue(state, null);
                                if((propValue == -1000) || (propValue == 1000))
                                {
                                    selectedControl = prop.Name;
                                    break;
                                }
                            }
                        }
                    }
                    else
                    {
                        bool[] buttons = state.GetButtons();
                        for(int i = 0; i < buttons.Length; i++)
                        {
                            if(buttons[i])
                            {
                                selectedControl = "Button " + i.ToString("00");
                            }
                        }
                    }
                }
                else // keyboard
                {
                    try
                    {
                        List<DirectInput.Key> keys = inputDevice.GetPressedKeys();
                        if(keys.Count > 0)
                        {
                            selectedControl = keys[0].ToString();
                        }
                    }
                    catch(DirectXException)
                    {
                        return;
                    }
                }

                if((selectedControl != null) &&
                    (selectedControl != inputPreviousSelectedControl))
                {
                    field.SetValue(Settings.Current.Players[inputSelectedPlayerIndex].Controls, selectedControl);
                    inputSelectedListViewItem.SubItems[1].Text = selectedControl;
                    lviControls_Leave(sender, e);
                }
                inputPreviousSelectedControl = selectedControl;

            }
        }
        #endregion

        private void doItButton_Click(object sender, System.EventArgs e)
        {
            sender.ToString();
        }


    }

    public class TestSettings
    {
        public static double Value1;
        public static double Value2;
        public static double Value3;
    }
}
