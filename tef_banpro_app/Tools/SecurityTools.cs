using System;
using System.Collections.Generic;
using System.Linq;

namespace tef_banpro_app.Tools
{
    public static class SecurityTools
    {
        private static readonly List<int> PatronClave = new List<int> {78, 79, 82, 77, 65, 76, 75, 69, 89, 78};

        /// <summary>
        /// Desencripta una cadena de caracteres que fue encriptada con el algoritmo de ICG
        /// </summary>
        /// <param name="encryptedPassword"></param>
        /// <returns></returns>
        public static string Decrypt(string encryptedPassword)
        {
            var listaEnteros = new List<int>();

            var cadena = encryptedPassword;

            while (cadena.Length > 0)
            {
                var extraido = cadena.Substring(0, 2); // Obtener los dos caracteres mas a la izquierda
                cadena = cadena.Remove(0, 2); // Remover los dos caracteres mas a la izquierda
                listaEnteros.Add(Convert.ToInt32(extraido, 16)); // Convertir de Hexadecimal a Decimal y agregar el valor a la lista
            }

            // Declarar una lista de enteros y alimentarla con las restas de los pares de valores de las cadenas PatronClave y listaEnteros
            var listaDec = PatronClave.Zip(listaEnteros, (parteIzquierda, parteDerecha) => parteDerecha - parteIzquierda ).ToList();

            // Declarar una lista de caracteres y alimentarla con los valores ASCII de cada entero
            var listaChar = new List<char>();
            foreach (var caracter in listaDec)
            {
                listaChar.Add((char) caracter);
            }

            // Concatener los elementos de la lista en una cadena
            return string.Join("", listaChar);
        }

        /// <summary>
        /// Encripta una cadena de caracteres con el algoritmo ICG
        /// </summary>
        /// <param name="plainPassword"></param>
        /// <returns></returns>
        public static string Encrypt(string plainPassword)
        {
            var lista = plainPassword.ToList(); // Obtener una lista de caracteres a partir de la cadena original

            // Crear una lista con los valores decimal (ASCII) correspondientes a cada caracter de la cadena original
            var listaInt = new List<int>();
            foreach (var caracter in lista)
            {
                listaInt.Add(caracter);
            }

            // Declarar una lista de enteros y alimentarla con las sumatorias de los pares de valores de las cadenas PatronClave y cadenaEnteros
            var listaDec = PatronClave
                .Zip(listaInt, (parteIzquierda, parteDerecha) => parteIzquierda + parteDerecha).ToList();

            // Declarar una lista de cadenas de caracteres y alimentarla con los valores Hexadecimales de cada cadena
            var listaHex = new List<string>();
            foreach (var caracter in listaDec)
            {
                listaHex.Add(caracter.ToString("X")); // Convertir a notacion Hexadecimal y agregar el valor a la lista
            }

            // Concatener los elementos de la lista en una cadena
            return string.Join("", listaHex);

        }
    }
}
