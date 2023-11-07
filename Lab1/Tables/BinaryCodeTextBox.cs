namespace Lab1.Tables;

public class BinaryCodeTextBox
{
    private readonly RichTextBox _textBox;

    public BinaryCodeTextBox(RichTextBox textBox)
    {
        _textBox = textBox;
    }

    public void AddRange(List<string> lines)
    {
        _textBox.AppendText(String.Join('\n', lines));
        _textBox.AppendText("\n");
    }

    public void Clear()
    {
        _textBox.Clear();
    }
}