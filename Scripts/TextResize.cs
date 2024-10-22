using Godot;
using System;

public partial class TextResize : Label
{
	public new string Text { get { return base.Text; } set { base.Text = value; ResizeText(); } }
	[Export] public int intendedStringLength = 3;
	float size;
	public override void _Ready()
	{
		string test = "";
		for(int i = 0; i < intendedStringLength; i++)
		{
			test += "_";
		}
		size = LabelSettings.Font.GetStringSize(test, HorizontalAlignment, -1, LabelSettings.FontSize).X;
		ResizeText();
	}
	public void ResizeText()
	{
		//GD.Print(LabelSettings.Font.GetStringSize(Text, HorizontalAlignment, -1, LabelSettings.FontSize).X);
		while(LabelSettings.Font.GetStringSize(Text, HorizontalAlignment, -1, LabelSettings.FontSize).X > size)
		{
			LabelSettings.FontSize -= 1;
		}
	}
}
