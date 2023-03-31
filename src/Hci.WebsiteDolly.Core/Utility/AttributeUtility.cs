using System;
using System.ComponentModel;
using System.Reflection;

namespace Hci.WebsiteDolly.Core.Utility
{
    public static class AttributeUtility
    {
        public static string GetDescription(Enum value)
        {
            try
            {
                FieldInfo fieldInfo = value.GetType().GetField(value.ToString());

                DescriptionAttribute[] attributes =
                    (DescriptionAttribute[])fieldInfo.GetCustomAttributes(typeof(DescriptionAttribute), false);

                return (attributes.Length > 0) ? attributes[0].Description : value.ToString();
            }
            catch
            {
                return value.ToString();
            }
        }
    }
}
