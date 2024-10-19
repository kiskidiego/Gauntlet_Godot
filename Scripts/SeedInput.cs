using Godot;
using System;

public partial class SeedInput : TextEdit
{
	public void OnTextChanged()
	{
		if (Text.Length > 0)
		{
			for (int i = 0; i < Text.Length; i++)
			{
				if (!((Text[i] >= '0' && Text[i] <= '9') || (Text[i] >= 'a' && Text[i] <= 'f')))
				{
					Text = Text.Remove(i, 1);
				}
			}
			if (Text.Length > 16)
			{
				Text = Text.Substring(0, 16);
			}
			Seed.seed = ConvertStringToDecimalFromHex(Text);
			GD.Print(Seed.seed);
		}
		else
		{
			Seed.seed = ulong.MaxValue;
		}
	}
	ulong ConvertStringToDecimalFromHex(string hex)
	{
		// Convert the number expressed in base-16 to an integer.
		ulong value = ulong.MaxValue;
		try
		{
			value = Convert.ToUInt64(hex, 16);
			GD.Print(value);
		}
		catch (Exception)
		{
		}
		return value;
	}
}
