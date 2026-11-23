using System;
using System.ComponentModel;
using System.Linq;
using System.Reflection;

namespace tef_banpro_app.Tools
{
    public static class Extensions
    {
        /// <summary>
        /// Retorna la descripcion de un a enumeracion
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static string GetDescription(this Enum value)
        {
            return
                value
                    .GetType()
                    .GetMember(value.ToString())
                    .FirstOrDefault()
                    ?.GetCustomAttribute<DescriptionAttribute>()
                    ?.Description;
        }

        /// <summary>
        /// Extrae una cadena de n caraceteres desde la derecha.
        /// </summary>
        public static string Right(this string value, int length)
        {
            return value.Substring(value.Length - length);
        }

        /// <summary>
        /// Extrae una cadena de n caraceteres desde la izquierda.
        /// </summary>
        public static string Left(this string value, int length)
        {
            return value.Substring(0, length);
        }
    }
}
