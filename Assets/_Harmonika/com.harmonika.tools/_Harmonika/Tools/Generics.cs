using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;
using NaughtyAttributes;
using UnityEngine.Windows;

namespace Harmonika.Tools
{
    #region Serializable Enums
    public enum Turn
    {
        on,
        off
    }
    
    public enum ParseableFields
    {
        none,
        cpf,
        cnpj,
        phone,
        cep,
        date
    }

    public enum BlockType
    {
        NoConnectionAndDueDate,
        LicenseNotActive
    }

    [System.Serializable]
    public enum AudioType
    {
        None,
        Music,
        SFX,
        MenuSound,
        Voice
    }

    public enum LeadInputType
    {
        InputField,
        Dropdown,
        Toggle
        //SearchableDropdown
    }

    public enum KeyboardType
    {

        AlphaLower,
        AlphaUpper,
        Symbols,
        Numeric,
        AlphaLowerEmail,
        AlphaUpperEmail,
        AlphaLowerNoSymbol,
        AlphaUpperNoSymbol,
    }

    public enum LeadID
    {
        cargo,
        autorizoContato,
        cep,
        cidade,
        cnpj,
        cpf,
        dataNascimento,
        email,
        empresa,
        estado,
        idade,
        id,
        pontos,
        nome,
        telefone,
        brinde,
        dataHora,
        tempo,
        ganhou,
        custom1,
        custom2,
        custom3,
        custom4,
        custom5
    }
    #endregion

    #region Serializable Classes
    [System.Serializable]
    public class Sound
    {
        public string name;
        public bool loop;
        public bool playOnAwake;
        public AudioClip clip;
        public AudioType type;
        [HideInInspector]
        public AudioSource audioSource;
        [Range(0f, 3f)]
        public float volume = 1;
        [Range(.1f, 3f)]
        public float pitch = 1;

    }

    [System.Serializable]
    public class MenuPanel
    {
        [SerializeField] private string name;
        [SerializeField] private CanvasGroup group;

        public string Name { get => name; }
        public CanvasGroup Group { get => group; }
    }


    [System.Serializable]
    public class MemoryGameConfig
    {
        public string cardBack;
        public List<string> cardsList;
        public string userLogo;
        public List<StorageItemConfig> storageItems;
        public List<LeadDataConfig> leadDataConfig;
        public string gameName;
        public string primaryColor;
        public string secondaryColor;
        public string tertiaryColor;
        public string neutralColor;
    }

    [System.Serializable]
    public class StorageItemConfig
    {
        [Header("Config")]
        public string _itemName;
        public int _initialValue;
        public int _prizeScore;
    }
    #endregion

    public static class HarmonikaConstants
    {
        public const string RESOURCES_PATH = "Assets/Resources/";
        public const string ANDROID_BUILD_PATH = "Builds/Android/";
    }

    public static class InvokeUtils
    {
        /// <summary>
        /// Invokes an Action after a delay, similar to MonoBehaviour's Invoke but using an Action instead of a string.
        /// </summary>
        /// <param name="action">The Action to execute after the delay.</param>
        /// <param name="delay">The delay time in seconds before invoking the Action.</param>
        public static void Invoke(Action action, float delay)
        {
            if (action == null)
            {
                Debug.LogError("InvokeUtility -> Invoke: Action cannot be null.");
                return;
            }

            GameObject tempObject = new GameObject("InvokeUtility_TempObject");
            InvokeRunner runner = tempObject.AddComponent<InvokeRunner>();
            runner.Run(action, delay);
        }

        private class InvokeRunner : MonoBehaviour
        {
            public void Run(Action action, float delay)
            {
                StartCoroutine(InvokeCoroutine(action, delay));
            }

            private System.Collections.IEnumerator InvokeCoroutine(Action action, float delay)
            {
                yield return new WaitForSeconds(delay);
                action.Invoke();
                Destroy(gameObject);
            }
        }
    }

    public static class HarmonikaExtension
    {
        /// <summary>
        /// Converts a hexadecimal string to a Color.
        /// </summary>
        /// <param name="color">The base color instance.</param>
        /// <param name="hex">The hexadecimal color string (e.g., "#FF5733").</param>
        /// <returns>A Color parsed from the hexadecimal string.</returns>
        public static Color HexToColor(this string hex)
        {
            ColorUtility.TryParseHtmlString(hex, out Color newColor);
            return newColor;
        }

        /// <summary>
        /// Shuffles the elements of a List<T> in place.
        /// </summary>
        /// <typeparam name="T">The type of elements in the list.</typeparam>
        /// <param name="list">The list to shuffle.</param>
        public static List<T> Shuffle<T>(this List<T> list)
        {
            for (int i = 0; i < list.Count; i++)
            {
                int randomIndex = UnityEngine.Random.Range(i, list.Count);

                T temp = list[i];
                list[i] = list[randomIndex];
                list[randomIndex] = temp;
            }
            return list;
        }

        /// <summary>
        /// Shuffles the elements of an Array<T> in place.
        /// </summary>
        /// <typeparam name="T">The type of elements in the array.</typeparam>
        /// <param name="array">The array to shuffle.</param>
        public static T[] Shuffle<T>(this T[] array)
        {
            for (int i = 0; i < array.Length; i++)
            {
                int randomIndex = UnityEngine.Random.Range(i, array.Length);

                T temp = array[i];
                array[i] = array[randomIndex];
                array[randomIndex] = temp;
            }
            return array;
        }

        /// <summary>
        /// Validates a Brazilian CPF number.
        /// </summary>
        /// <param name="value">The CPF number as a string (can contain dots and dashes or just digits).</param>
        /// <returns>True if the CPF is valid, otherwise false.</returns>
        public static bool ValidateCPF(this string value)
        {
            // Remove caracteres não numéricos
            string cpf = new string(value.Where(char.IsDigit).ToArray());

            // O CPF deve ter 11 dígitos e não pode ser uma sequência de números idênticos
            if (cpf.Length != 11 || cpf.All(c => c == cpf[0]))
                return false;

            // Calcula o primeiro dígito verificador
            int sum = 0;
            for (int i = 0; i < 9; i++)
                sum += (cpf[i] - '0') * (10 - i);

            int firstDigit = (sum * 10) % 11;
            if (firstDigit == 10) firstDigit = 0;

            // Verifica o primeiro dígito
            if (firstDigit != (cpf[9] - '0'))
                return false;

            // Calcula o segundo dígito verificador
            sum = 0;
            for (int i = 0; i < 10; i++)
                sum += (cpf[i] - '0') * (11 - i);

            int secondDigit = (sum * 10) % 11;
            if (secondDigit == 10) secondDigit = 0;

            // Verifica o segundo dígito
            return secondDigit == (cpf[10] - '0');
        }

        /// <summary>
        /// Validates a Brazilian CNPJ number.
        /// </summary>
        /// <param name="value">The CNPJ number as a string, which may include dots, dashes, and slashes or contain only digits.</param>
        /// <returns>True if the CNPJ is valid according to the Brazilian standard, otherwise false.</returns>
        public static bool ValidateCNPJ(this string value)
        {
            // Remove caracteres não numéricos
            string cnpj = new string(value.Where(char.IsDigit).ToArray());

            // CNPJ precisa ter 14 digitos e não ser uma sequência de números idênticos
            if (cnpj.Length != 14 || cnpj.All(c => c == cnpj[0]))
                return false;

            // Arrays de multiplicadores para os cálculos dos dígitos verificadores
            int[] multiplicador1 = { 5, 4, 3, 2, 9, 8, 7, 6, 5, 4, 3, 2 };
            int[] multiplicador2 = { 6, 5, 4, 3, 2, 9, 8, 7, 6, 5, 4, 3, 2 };

            // Calcular o primeiro dígito verificador
            int sum = 0;
            for (int i = 0; i < 12; i++)
                sum += (cnpj[i] - '0') * multiplicador1[i];

            int firstDigit = (sum % 11) < 2 ? 0 : 11 - (sum % 11);

            // Verificar o primeiro dígito
            if (firstDigit != (cnpj[12] - '0'))
                return false;

            // Calcular o segundo dígito verificador
            sum = 0;
            for (int i = 0; i < 13; i++)
                sum += (cnpj[i] - '0') * multiplicador2[i];

            int secondDigit = (sum % 11) < 2 ? 0 : 11 - (sum % 11);

            // Verificar o segundo dígito
            return secondDigit == (cnpj[13] - '0');
        }

        /// <summary>
        /// Validates a Brazilian phone number, ensuring it is in a standard format.
        /// </summary>
        /// <param name="phone">The phone number as a string, which may contain digits, parentheses, dashes, or spaces.</param>
        /// <returns>True if the phone number has a valid format (10 or 11 digits with DDD, or 8 or 9 digits without DDD), otherwise false.</returns>
        public static bool ValidatePhone(this string phone)
        {
            if (phone.Any(char.IsLetter))
                return false;

            string digits = new string(phone.Where(char.IsDigit).ToArray());

            return digits.Length >= 10;
        }

        /// <summary>
        /// Validates an email address format based on standard email structure.
        /// </summary>
        /// <param name="email">The email address as a string.</param>
        /// <returns>True if the email matches a standard format, otherwise false.</returns>
        public static bool ValidateEmail(this string email)
        {
            if (string.IsNullOrWhiteSpace(email))
                return false;

            string pattern = @"^[^@\s]+@[^@\s]+\.[^@\s]+$";
            return Regex.IsMatch(email, pattern, RegexOptions.IgnoreCase);
        }

        public static string Format(this string str, ParseableFields parseableField)
        {
            if (parseableField != ParseableFields.none) str = Regex.Replace(str, @"\D", "");

            switch (parseableField)
            {
                case ParseableFields.cpf:
                    if (str.Length < 11)
                        return str;

                    else if (str.Length >= 11)
                        str = str.Substring(0, 11);
                    return Regex.Replace(str, @"(\d{3})(\d{0,3})(\d{0,3})(\d{0,2})", "$1.$2.$3-$4");


                case ParseableFields.cnpj:
                    if (str.Length < 14)
                        return str;

                    if (str.Length >= 14)
                        str = str.Substring(0, 14);

                    return Regex.Replace(str, @"(\d{2})(\d{0,3})(\d{0,3})(\d{0,4})(\d{0,2})", "$1.$2.$3/$4-$5");

                case ParseableFields.phone:
                    if (str.Length < 11)
                        return str;

                    if (str.Length >= 11)
                        str = str.Substring(0, 11);

                    return Regex.Replace(str, @"(\d{2})(\d{0,5})(\d{0,4})", "($1) $2-$3");

                case ParseableFields.cep:
                    if (str.Length < 8)
                        return str;

                    if (str.Length >= 8)
                        str = str.Substring(0, 8);

                    return Regex.Replace(str, @"(\d{5})(\d{0,3})", "$1-$2");

                case ParseableFields.date:
                    if (str.Length < 8)
                        return str;

                    if (str.Length >= 8)
                        str = str.Substring(0, 8);

                    return Regex.Replace(str, @"(\d{2})(\d{0,2})(\d{0,4})", "$1/$2/$3");

                default:
                    return str;
            }
        }
    }
}