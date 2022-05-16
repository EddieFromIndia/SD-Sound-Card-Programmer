namespace SD_Sound_Card_Programmer.Services;

public static class NameConverter
{
    public static string ToFormalName(string name)
    {
        name = name[0..^4].Replace("-", " ");
        return Regex.Replace(name, @"(^\w)|(\s\w)", c => c.Value.ToUpper());
    }
}
