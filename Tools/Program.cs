// =============================================
// Proyecto: GECO.Tools (Aplicación de Consola .NET 8)
// Archivo: Program.cs
// =============================================
// Este debe ser un proyecto SEPARADO tipo Console App
// Agregar referencia al proyecto GECO.Utility
// =============================================

using System;
using Utility;

namespace GECO.Tools
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("========================================");
            Console.WriteLine("GECO - Generador de Hash de Contraseña");
            Console.WriteLine("========================================");
            Console.WriteLine();

            // Contraseña por defecto del administrador
            string passwordDefault = "Admin123!";

            Console.WriteLine("Generando hash para la contraseña por defecto...");
            Console.WriteLine($"Password: {passwordDefault}");
            Console.WriteLine();

            // Generar hash
            string hash = SecurityHelper.HashPassword(passwordDefault);

            Console.WriteLine("Hash generado exitosamente:");
            Console.WriteLine();
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine(hash);
            Console.ResetColor();
            Console.WriteLine();

            Console.WriteLine("========================================");
            Console.WriteLine("INSTRUCCIONES:");
            Console.WriteLine("========================================");
            Console.WriteLine("1. Copiar el hash generado (línea verde)");
            Console.WriteLine("2. Abrir el archivo 02_CrearUsuarioAdmin.sql");
            Console.WriteLine("3. Reemplazar 'CAMBIAR_POR_HASH_REAL' por el hash copiado");
            Console.WriteLine("4. Ejecutar el script SQL");
            Console.WriteLine();
            Console.WriteLine("Credenciales del administrador:");
            Console.WriteLine($"  Usuario: admin");
            Console.WriteLine($"  Password: {passwordDefault}");
            Console.WriteLine();
            Console.WriteLine("¡IMPORTANTE! Cambiar la contraseña después del primer login");
            Console.WriteLine("========================================");
            Console.WriteLine();

            // Opción para generar hash personalizado
            Console.WriteLine("¿Deseas generar un hash para otra contraseña? (S/N)");
            string respuesta = Console.ReadLine();

            if (respuesta?.ToUpper() == "S")
            {
                Console.WriteLine();
                Console.Write("Ingresa la contraseña: ");
                string customPassword = LeerPasswordOculto();
                Console.WriteLine();

                // Validar fortaleza
                string errorValidacion = SecurityHelper.ValidarFortalezaPassword(customPassword);
                if (errorValidacion != null)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine($"Error: {errorValidacion}");
                    Console.ResetColor();
                }
                else
                {
                    string customHash = SecurityHelper.HashPassword(customPassword);
                    Console.WriteLine();
                    Console.WriteLine("Hash generado:");
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine(customHash);
                    Console.ResetColor();
                }
            }

            Console.WriteLine();
            Console.WriteLine("Presiona cualquier tecla para salir...");
            Console.ReadKey();
        }

        /// <summary>
        /// Lee una contraseña ocultando los caracteres
        /// </summary>
        static string LeerPasswordOculto()
        {
            string password = "";
            ConsoleKeyInfo key;

            do
            {
                key = Console.ReadKey(true);

                if (key.Key != ConsoleKey.Backspace && key.Key != ConsoleKey.Enter)
                {
                    password += key.KeyChar;
                    Console.Write("*");
                }
                else if (key.Key == ConsoleKey.Backspace && password.Length > 0)
                {
                    password = password.Substring(0, password.Length - 1);
                    Console.Write("\b \b");
                }
            }
            while (key.Key != ConsoleKey.Enter);

            return password;
        }
    }
}