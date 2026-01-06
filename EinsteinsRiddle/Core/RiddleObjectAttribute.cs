namespace EinsteinsRiddle.Core
{
    public class RiddleObjectAttribute
    {
        public string Key { get; set; }
        public string Value { get; set; }

        public bool IsSet => !string.IsNullOrEmpty(Value);

        public RiddleObjectAttribute(string key, string val)
        {
            Key = key;
            Value = val;
        }

        public override string ToString()
        {
            var val = string.Empty;
            if (string.IsNullOrEmpty(Value))
            {
                val += "???";
            }
            else if (Value.Length >= 3)
            {
                val += Value.Substring(0, 3);
            }
            else if (Value.Length == 1)
            {
                val += $" {Value} ";
            }
            else if (Value.Length == 2)
            {
                val += $"{Value} ";
            }

            return $"{Key}: {val}";
        }

        public override bool Equals(object obj)
        {
            if (obj is RiddleObjectAttribute)
            {
                var casted = obj as RiddleObjectAttribute;

                return casted.Key == Key && casted.Value == Value;
            }

            return false;
        }

        public override int GetHashCode()
        {
            var keyHash = Key.GetHashCode();
            var valHash = Value.GetHashCode();

            return (keyHash * 10) + valHash;
        }
    }
}
