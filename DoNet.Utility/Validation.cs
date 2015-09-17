using System;

namespace DoNet.Utility
{
    public static class Validation
    {
        public static ValidationHelper<T> InitValidation<T>(this T value, string name)
        {
            return new ValidationHelper<T>(value, name);
        }

        public static ValidationHelper<string> NotEmpty(this ValidationHelper<string> current)
        {
            current.NotDefault();
            if (string.IsNullOrEmpty(current.Value))
                throw new ArgumentException(string.Format("{0}����Ϊ�գ�", current.Name));
            return current;
        }

        public static ValidationHelper<string> LongerThan(this ValidationHelper<string> current, int length)
        {
            current.NotDefault();
            if (current.Value.Length < length)
                throw new ArgumentException(string.Format("{0}�ĳ��Ȳ���С��{1}��", current.Name, length));
            return current;
        }

        public static ValidationHelper<string> ShorterThan(this ValidationHelper<string> current, int length)
        {
            current.NotDefault();
            if (current.Value.Length > length)
                throw new ArgumentException(string.Format("{0}�ĳ��Ȳ��ɳ���{1}��", current.Name, length));
            return current;
        }

        public static ValidationHelper<string> LengthBetween(this ValidationHelper<string> current, int minLength,
            int maxLength)
        {
            current.NotDefault();
            if (current.Value.Length < minLength || current.Value.Length > maxLength)
                throw new ArgumentException(string.Format("{0}�ĳ��ȱ�����{1}��{2}֮�䣡", current.Name, minLength, maxLength));
            return current;
        }

        public static ValidationHelper<int> IsNum(this ValidationHelper<string> current)
        {
            int result;
            if (!int.TryParse(current.Value, out result))
                throw new ArgumentException(string.Format("{0}����Ϊ���֣�", current.Name));

            return new ValidationHelper<int>(result, current.Name);
        }

        public static ValidationHelper<int> LargerThan(this ValidationHelper<int> current, int num)
        {
            if (current.Value < num)
                throw new ArgumentException(string.Format("{0}��ֵ����С��{1}��", current.Name, num));
            return current;
        }

        public static ValidationHelper<int> SmallerThan(this ValidationHelper<int> current, int num)
        {
            if (current.Value > num)
                throw new ArgumentException(string.Format("{0}��ֵ���ɴ���{1}��", current.Name, num));
            return current;
        }
    }
}