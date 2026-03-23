using System;
using System.Text.RegularExpressions;

namespace SistemaGestionCRA.Logica
{
    public static class ValidadorRut
    {
        public static bool ValidarRut(string rut)
        {
            if (string.IsNullOrWhiteSpace(rut)) return false;

            rut = rut.Replace(".", "").Replace("-", "").ToUpper();

            if (!Regex.IsMatch(rut, @"^\d{7,8}[0-9K]$"))
            {
                return false;
            }

            string dvStr = rut.Substring(rut.Length - 1);
            string cuerpo = rut.Substring(0, rut.Length - 1);

            if (!int.TryParse(cuerpo, out int rutInt))
            {
                return false;
            }

            return dvStr == CalcularDv(rutInt);
        }

        public static string CalcularDv(int rut)
        {
            int suma = 0;
            int factor = 2;

            while (rut > 0)
            {
                suma += (rut % 10) * factor;
                rut /= 10;
                factor = factor == 7 ? 2 : factor + 1;
            }

            int residuo = suma % 11;
            int resultado = 11 - residuo;

            return resultado switch
            {
                11 => "0",
                10 => "K",
                _ => resultado.ToString()
            };
        }

        public static string FormatearRut(string rut)
        {
            if (string.IsNullOrWhiteSpace(rut)) return "";
            rut = rut.Replace(".", "").Replace("-", "").ToUpper();
            if (rut.Length < 2) return rut;

            string dv = rut.Substring(rut.Length - 1);
            string cuerpo = rut.Substring(0, rut.Length - 1);

            return string.Format("{0:N0}-{1}", long.Parse(cuerpo), dv).Replace(",", ".");
        }
    }
}
