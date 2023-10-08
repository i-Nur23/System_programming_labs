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
        _textBox.Text = String.Join('\n', lines);
    }
}