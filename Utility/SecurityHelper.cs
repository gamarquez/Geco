using System.Security.Cryptography;
using System.Text;

namespace Utility
{
    /// <summary>
    /// Helper para funciones de seguridad y encriptación
    /// </summary>
    public static class SecurityHelper
    {
        private const int SaltSize = 16; // 128 bits
        private const int HashSize = 32; // 256 bits
        private const int Iterations = 100000; // Iteraciones PBKDF2

        /// <summary>
        /// Genera un hash seguro de la contraseña usando PBKDF2
        /// </summary>
        /// <param name="password">Contraseña en texto plano</param>
        /// <returns>Hash en formato: {iterations}.{salt}.{hash}</returns>
        public static string HashPassword(string password)
        {
            if (string.IsNullOrWhiteSpace(password))
                throw new ArgumentException("La contraseña no puede estar vacía", nameof(password));

            // Generar salt aleatorio
            using (var rng = RandomNumberGenerator.Create())
            {
                byte[] salt = new byte[SaltSize];
                rng.GetBytes(salt);

                // Generar hash
                byte[] hash = GenerateHash(password, salt, Iterations);

                // Retornar en formato: iterations.salt.hash (todo en Base64)
                return $"{Iterations}.{Convert.ToBase64String(salt)}.{Convert.ToBase64String(hash)}";
            }
        }

        /// <summary>
        /// Verifica si una contraseña coincide con un hash almacenado
        /// </summary>
        /// <param name="password">Contraseña a verificar</param>
        /// <param name="hashedPassword">Hash almacenado</param>
        /// <returns>True si la contraseña es correcta</returns>
        public static bool VerifyPassword(string password, string hashedPassword)
        {
            if (string.IsNullOrWhiteSpace(password))
                return false;

            if (string.IsNullOrWhiteSpace(hashedPassword))
                return false;

            try
            {
                // Separar el hash almacenado
                var parts = hashedPassword.Split('.');
                if (parts.Length != 3)
                    return false;

                int iterations = int.Parse(parts[0]);
                byte[] salt = Convert.FromBase64String(parts[1]);
                byte[] storedHash = Convert.FromBase64String(parts[2]);

                // Generar hash con la contraseña ingresada
                byte[] testHash = GenerateHash(password, salt, iterations);

                // Comparación constante en tiempo para prevenir timing attacks
                return ConstantTimeComparison(storedHash, testHash);
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Genera el hash PBKDF2 de una contraseña
        /// </summary>
        private static byte[] GenerateHash(string password, byte[] salt, int iterations)
        {
            using (var pbkdf2 = new Rfc2898DeriveBytes(password, salt, iterations, HashAlgorithmName.SHA256))
            {
                return pbkdf2.GetBytes(HashSize);
            }
        }

        /// <summary>
        /// Comparación en tiempo constante para prevenir timing attacks
        /// </summary>
        private static bool ConstantTimeComparison(byte[] a, byte[] b)
        {
            if (a.Length != b.Length)
                return false;

            int result = 0;
            for (int i = 0; i < a.Length; i++)
            {
                result |= a[i] ^ b[i];
            }

            return result == 0;
        }

        /// <summary>
        /// Valida la fortaleza de una contraseña
        /// </summary>
        /// <param name="password">Contraseña a validar</param>
        /// <returns>Mensaje de error o null si es válida</returns>
        public static string ValidarFortalezaPassword(string password)
        {
            if (string.IsNullOrWhiteSpace(password))
                return "La contraseña no puede estar vacía";

            if (password.Length < 8)
                return "La contraseña debe tener al menos 8 caracteres";

            bool tieneMayuscula = false;
            bool tieneMinuscula = false;
            bool tieneNumero = false;
            bool tieneEspecial = false;

            foreach (char c in password)
            {
                if (char.IsUpper(c)) tieneMayuscula = true;
                else if (char.IsLower(c)) tieneMinuscula = true;
                else if (char.IsDigit(c)) tieneNumero = true;
                else tieneEspecial = true;
            }

            if (!tieneMayuscula)
                return "La contraseña debe contener al menos una letra mayúscula";

            if (!tieneMinuscula)
                return "La contraseña debe contener al menos una letra minúscula";

            if (!tieneNumero)
                return "La contraseña debe contener al menos un número";

            if (!tieneEspecial)
                return "La contraseña debe contener al menos un carácter especial";

            return null; // Contraseña válida
        }

        /// <summary>
        /// Genera una contraseña temporal aleatoria
        /// </summary>
        public static string GenerarPasswordTemporal(int longitud = 12)
        {
            const string caracteresPermitidos = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789!@#$%&*";

            using (var rng = RandomNumberGenerator.Create())
            {
                byte[] randomBytes = new byte[longitud];
                rng.GetBytes(randomBytes);

                StringBuilder password = new StringBuilder(longitud);
                foreach (byte b in randomBytes)
                {
                    password.Append(caracteresPermitidos[b % caracteresPermitidos.Length]);
                }

                return password.ToString();
            }
        }
    }
}

