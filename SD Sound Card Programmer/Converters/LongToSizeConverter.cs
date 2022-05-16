namespace SD_Sound_Card_Programmer.Converters;

[ValueConversion(typeof(long), typeof(string))]
public class LongToSizeConverter : IValueConverter
{
    public static LongToSizeConverter Instance = new();

    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if ((long)value > Math.Pow(1024, 3))
        {
            return ((long)value / (Math.Pow(1024, 3))).ToString("0.##") + " GB";
        }
        else if ((long)value > Math.Pow(1024, 2))
        {
            return ((long)value / (Math.Pow(1024, 2))).ToString("0.##") + " MB";
        }
        else if ((long)value > 1024)
        {
            return ((long)value / 1024).ToString("0.##") + " KB";
        }
        else if ((long)value < 0)
        {
            return "0.00 B";
        }
        else
        {
            return ((long)value).ToString("0.##") + " B";
        }
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
