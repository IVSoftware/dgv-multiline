
using System.ComponentModel;
using System.Diagnostics;

namespace dgv_multiline
{
    public partial class MainForm : Form
    {
        public MainForm() => InitializeComponent();
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            dataGridView.DataSource = Records;
            dataGridView.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dataGridView.DefaultCellStyle.WrapMode = DataGridViewTriState.True;
            dataGridView.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.AllCells;
            dataGridView.CellValidating += (sender, e) =>
            {
                Debug.WriteLine(
                    $"Ensure that only one cell validation per commit {dataGridView.CurrentCell?.Value}");
            };
            dataGridView.EditingControlShowing += (sender, e) =>
            {
                if (e.Control is TextBox textBox)
                {
                    _revertText = textBox.Text;
                    textBox.TextChanged -= localOnTextChanged;
                    textBox.PreviewKeyDown -= localPreviewKeyDown;
                    textBox.TextChanged += localOnTextChanged;
                    textBox.PreviewKeyDown += localPreviewKeyDown;

                    void localOnTextChanged(object? sender, EventArgs e)
                    {
                        BeginInvoke(() =>
                        {
                            dataGridView.CurrentCell.Value = textBox.Text;
                        });
                    }
                    void localPreviewKeyDown(object? sender, PreviewKeyDownEventArgs e)
                    {
                        if(e.KeyData == Keys.Escape)
                        {
                            dataGridView.CurrentCell.Value = _revertText;
                        }
                    }
                }
            };
            Records.Add(new Record());
        }
        string _revertText = string.Empty;
        BindingList<Record> Records = new BindingList<Record>();
    }
    class Record
    {
        public string? Description { get; set; }
    }
}
