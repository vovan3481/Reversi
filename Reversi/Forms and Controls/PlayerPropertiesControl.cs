using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using Reversi.Classes;

namespace Reversi.Controls
{
    public partial class PlayerPropertiesControl : UserControl
    {
        #region Constructors

        public PlayerPropertiesControl()
        {
            this.InitializeComponent();

            this.LoadPlayerTypes();
            this.LoadMaxDepth();
        }

        #endregion

        #region Properties

        public string PlayerColor
        {
            get
            {
                return this.grbPlayerProperties.Text;               
            }
            set
            {
                this.grbPlayerProperties.Text = value;
            }
        }

        public PlayerType Type
        {
            get
            {
                return (PlayerType)this.cmbType.SelectedItem;                
            }
            set
            {
                this.cmbType.SelectedItem = value;
            }
        }

        public string PlayerName
        {
            get
            {
                return this.txtName.Text;
            }
            set
            {
                this.txtName.Text = value;
            }
        }

        public int MaxDepth
        {
            get
            {
                return (int)this.cmbMaxDepth.SelectedItem;
            }
            set
            {
                this.cmbMaxDepth.SelectedItem = value;
            }
        }

        #endregion

        #region Methods

        private void LoadPlayerTypes()
        {
            this.cmbType.Items.Clear();

            this.cmbType.Items.Add(PlayerType.Компьютер);
            this.cmbType.Items.Add(PlayerType.Человек);

            this.cmbType.SelectedIndex = 0;
        }

        private void LoadMaxDepth()
        {
            this.cmbMaxDepth.Items.Clear();
            for (int depth = ComputerPlayer.MinDepth; depth <= ComputerPlayer.MaxDepth; depth++)
            {
                this.cmbMaxDepth.Items.Add(depth);
            }
            this.cmbMaxDepth.SelectedIndex = 0;
        }
        //проверка заполнения
        public bool AreControlValuesCorrect()
        {
            if (this.cmbType.SelectedItem == null)
            {
                MessageBox.Show("Вы забыли выбрать тип игрока");
                this.cmbType.Select();
                return false;
            }
            if (this.txtName.Text.Length == 0)
            {
                MessageBox.Show("Вы забыли ввести имя");
                this.txtName.Select();
                return false;
            }
            if (this.cmbMaxDepth.SelectedItem == null)
            {
                MessageBox.Show("Вы забыли выбрать глубину");
                this.cmbMaxDepth.Select();
                return false;
            }

            return true;
        }

        #endregion

        #region Events Handling

        private void cmbType_SelectedIndexChanged(object sender, EventArgs e)
        {
            this.lblMaxDepth.Enabled = (this.Type == PlayerType.Компьютер);
            this.cmbMaxDepth.Enabled = this.lblMaxDepth.Enabled;
        }

        #endregion

        private void grbPlayerProperties_Enter(object sender, EventArgs e)
        {

        }
    }
}