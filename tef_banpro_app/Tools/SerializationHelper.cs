using System.IO;
using System.Xml.Serialization;

namespace tef_banpro_app.Tools
{
    public static class SerializationHelper
    {
        /// <summary>
        /// Deserealiza los datos de entrada trasnformandolos en objetos 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="entrada">cadena a deserealizar (convertir a objetos)</param>
        /// <returns></returns>
        public static T Deserealize<T>(string entrada) where T : class
        {
            var ser = new XmlSerializer(typeof(T));

            using (var sr = new StringReader(entrada))
            {
                return (T)ser.Deserialize(sr);
            }
        }

        /// <summary>
        /// Transforma el objeto dato en datos XML.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="objeto">Objeto a serializar (convertir en datos XML)</param>
        /// <returns></returns>
        public static string Serialize<T>(T objeto)
        {
            var xmlSerializer = new XmlSerializer(objeto.GetType());

            using (var textWriter = new StringWriter())
            {
                xmlSerializer.Serialize(textWriter, objeto);

                return textWriter.ToString();
            }
        }
    }
}
