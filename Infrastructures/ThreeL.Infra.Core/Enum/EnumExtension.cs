using System.ComponentModel;
using System.Reflection;

namespace ThreeL.Infra.Core.Enum
{
    public static class EnumExtension
    {
        public static string GetDescription(this System.Enum em)
        {
            Type type = em.GetType();
            FieldInfo fd = type.GetField(em.ToString());
            if (fd == null)
                return string.Empty;
            object[] attrs = fd.GetCustomAttributes(typeof(DescriptionAttribute), false);
            string name = string.Empty;
            foreach (DescriptionAttribute attr in attrs)
            {
                name = attr.Description;
            }

            return name;
        }
    }
}
