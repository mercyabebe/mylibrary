using System;
using System.Data;
using System.Windows.Forms;

namespace MyLibrary
{
    public partial class ReportForm : Form
    {
        public ReportForm(string reportTitle, DataTable reportData)
        {
            InitializeComponent();
            this.Text = reportTitle;
            this.lblTitle.Text = reportTitle;
            this.dgvReport.DataSource = reportData;

            // Auto-size columns to fit data
            this.dgvReport.AutoResizeColumns(DataGridViewAutoSizeColumnsMode.AllCells);
        }

        private void btnClose_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}