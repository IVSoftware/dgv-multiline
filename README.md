Here's one way that worked for me in testing. Ordinarily, the underlying cell value isn't changing when the text in the editing control does. The value is copied on a commit, and cancelling the operation simply skips the copy operation.

But by changing the underlying cell value in response to changes in the editor control, then the row height tracks because of the `AutoSizeRows` setting. If the edit is cancelled, however (e.g. the [Escape] key is pressed) it's now our responsibility to revert the cell value using the `_revertText` value that was pushed when the editor was shown.

___

##### Preconditions:

 - `DefaultCellStyle.WrapMode` is set to `DataGridViewTriState.True`
 - `AutoSizeRowsMode` is set to `DataGridViewAutoSizeRowsMode.AllCells`
 - **[Shift][Enter]** to add new line

[![screenshot][1]][1]

```
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
        dataGridView.EditingControlShowing += (sender, e) =>
        {
            if (e.Control is TextBox textBox)
            {
                Debug.Assert(textBox.Multiline, "Already true, and this is due to WrapMode property setting");
                _revertText = textBox.Text;
                textBox.TextChanged -= localOnTextChanged;
                textBox.PreviewKeyDown -= localPreviewKeyDown;
                textBox.TextChanged += localOnTextChanged;
                textBox.PreviewKeyDown += localPreviewKeyDown;

                void localOnTextChanged(object? sender, EventArgs e)
                {
                    BeginInvoke(() => // Because drawing artifacts have been known to occur otherwise.
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
        dataGridView.CellValidating += (sender, e) =>
        {
            Debug.WriteLine(
                $"Ensure that, regardless, only one cell validation happens per commit {dataGridView.CurrentCell?.Value}");
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
```


  [1]: https://i.sstatic.net/WxBNdeOw.png