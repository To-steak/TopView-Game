using System.Collections.Generic;
using System.Text;
using Microsoft.CodeAnalysis;

namespace TopView.Analyzers
{
    /// <summary>
    /// 이름 규칙 검사에 쓰는 형식 판정, 단어 나누기, 외부 이름 판정을 모은다.
    /// </summary>
    public static class NamingUtility
    {
        // 줄임말과 소문자로 쓴 머리글자 약어. 단어 하나와 정확히 같을 때만 걸린다
        // Max, Min은 규칙 문서의 예시(MAX_HEALTH)가 쓰므로 넣지 않는다
        private static readonly HashSet<string> _abbreviations = new HashSet<string>
        {
            "anim", "arr", "atk", "btn", "buf", "calc", "cam", "cnt", "col", "ctrl", "ctx", "cur", "curr",
            "desc", "dest", "dir", "dmg", "dst", "elem", "err", "fn", "func", "idx", "img", "impl", "info",
            "init", "lbl", "len", "mgr", "ms", "msg", "num", "obj", "param", "pos", "prev", "proc", "pt",
            "res", "ret", "rot", "sec", "spd", "src", "str", "temp", "tmp", "txt", "util", "utils", "val", "vel",
            "ai", "fov", "fx", "hp", "id", "mp", "sfx", "ui", "ux", "vfx",
        };

        private static readonly HashSet<string> _booleanPrefixes = new HashSet<string> { "is", "has", "can" };

        /// <summary>
        /// PascalCase인지 확인한다. 대문자로 시작하고 밑줄이 없다.
        /// </summary>
        /// <param name="name">검사할 이름</param>
        /// <returns>PascalCase면 true</returns>
        public static bool IsPascalCase(string name)
        {
            return name.Length > 0 && char.IsUpper(name[0]) && IsLettersAndDigits(name, 1);
        }

        /// <summary>
        /// camelCase인지 확인한다. 소문자로 시작하고 밑줄이 없다.
        /// </summary>
        /// <param name="name">검사할 이름</param>
        /// <returns>camelCase면 true</returns>
        public static bool IsCamelCase(string name)
        {
            return name.Length > 0 && char.IsLower(name[0]) && IsLettersAndDigits(name, 1);
        }

        /// <summary>
        /// _camelCase인지 확인한다. 밑줄 하나 뒤에 camelCase가 온다.
        /// </summary>
        /// <param name="name">검사할 이름</param>
        /// <returns>_camelCase면 true</returns>
        public static bool IsPrivateFieldCase(string name)
        {
            return name.Length > 1 && name[0] == '_' && IsCamelCase(name.Substring(1));
        }

        /// <summary>
        /// UPPER_SNAKE_CASE인지 확인한다. 대문자로 시작하고 대문자, 숫자, 단어 사이 밑줄만 쓴다.
        /// </summary>
        /// <param name="name">검사할 이름</param>
        /// <returns>UPPER_SNAKE_CASE면 true</returns>
        public static bool IsUpperSnakeCase(string name)
        {
            if (name.Length == 0 || !char.IsUpper(name[0]) || name[name.Length - 1] == '_')
            {
                return false;
            }

            for (int index = 0; index < name.Length; index++)
            {
                char character = name[index];
                if (character == '_')
                {
                    if (name[index - 1] == '_')
                    {
                        return false;
                    }

                    continue;
                }

                if (!char.IsDigit(character) && !(char.IsLetter(character) && char.IsUpper(character)))
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// 이름을 단어로 나눈다. 밑줄, 소문자 뒤 대문자, 약어 뒤 새 단어, 글자와 숫자 사이에서 나눈다.
        /// 예: <c>_enemyUIState2</c> → <c>enemy</c>, <c>UI</c>, <c>State</c>, <c>2</c>
        /// </summary>
        /// <param name="name">나눌 이름</param>
        /// <returns>나눈 단어 목록</returns>
        public static List<string> SplitWords(string name)
        {
            List<string> words = new List<string>();
            StringBuilder current = new StringBuilder();
            for (int index = 0; index < name.Length; index++)
            {
                char character = name[index];
                if (character == '_')
                {
                    FlushWord(words, current);
                    continue;
                }

                if (current.Length > 0 && IsWordBoundary(name, index))
                {
                    FlushWord(words, current);
                }

                current.Append(character);
            }

            FlushWord(words, current);
            return words;
        }

        /// <summary>
        /// 이름에 들어 있는 금지 줄임말을 찾는다.
        /// </summary>
        /// <param name="name">검사할 이름</param>
        /// <param name="isUpperWordSkipped">대문자로만 된 단어를 건너뛸지. 머리글자 약어 검사가 따로 잡을 때 true</param>
        /// <returns>처음 찾은 줄임말. 없으면 null</returns>
        public static string FindAbbreviation(string name, bool isUpperWordSkipped)
        {
            foreach (string word in SplitWords(name))
            {
                if (isUpperWordSkipped && word.Length >= 2 && IsAllUpperLetters(word))
                {
                    continue;
                }

                if (_abbreviations.Contains(word.ToLowerInvariant()))
                {
                    return word;
                }
            }

            return null;
        }

        /// <summary>
        /// 이름에 대문자 두 글자 이상으로 된 단어(머리글자 약어)가 있는지 확인한다.
        /// </summary>
        /// <param name="name">검사할 이름</param>
        /// <returns>머리글자 약어가 있으면 true</returns>
        public static bool HasAcronym(string name)
        {
            foreach (string word in SplitWords(name))
            {
                if (word.Length >= 2 && IsAllUpperLetters(word))
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// bool 이름이 is, has, can 중 하나로 시작하는지 확인한다.
        /// </summary>
        /// <param name="name">검사할 이름</param>
        /// <returns>접두사가 있으면 true</returns>
        public static bool HasBooleanPrefix(string name)
        {
            List<string> words = SplitWords(name);
            return words.Count > 1 && _booleanPrefixes.Contains(words[0].ToLowerInvariant());
        }

        /// <summary>
        /// 외부 API가 이름을 정한 심볼인지 확인한다. 재정의, 인터페이스 구현, Unity 메시지와 그 매개변수가 해당한다.
        /// </summary>
        /// <param name="symbol">검사할 심볼</param>
        /// <returns>이름을 바꿀 수 없는 심볼이면 true</returns>
        public static bool IsExternallyNamed(ISymbol symbol)
        {
            if (symbol is IParameterSymbol parameter)
            {
                return parameter.ContainingSymbol != null && IsExternallyNamed(parameter.ContainingSymbol);
            }

            if (symbol.IsOverride)
            {
                return true;
            }

            if (symbol is IMethodSymbol method)
            {
                if (method.ExplicitInterfaceImplementations.Length > 0 || UnityMessageUtility.IsUnityMessage(method))
                {
                    return true;
                }
            }
            else if (symbol is IPropertySymbol property)
            {
                if (property.ExplicitInterfaceImplementations.Length > 0)
                {
                    return true;
                }
            }
            else if (symbol is IEventSymbol eventSymbol)
            {
                if (eventSymbol.ExplicitInterfaceImplementations.Length > 0)
                {
                    return true;
                }
            }
            else
            {
                return false;
            }

            return ImplementsInterfaceMember(symbol);
        }

        private static bool ImplementsInterfaceMember(ISymbol symbol)
        {
            INamedTypeSymbol containingType = symbol.ContainingType;
            if (containingType == null)
            {
                return false;
            }

            foreach (INamedTypeSymbol interfaceType in containingType.AllInterfaces)
            {
                foreach (ISymbol interfaceMember in interfaceType.GetMembers(symbol.Name))
                {
                    ISymbol implementation = containingType.FindImplementationForInterfaceMember(interfaceMember);
                    if (SymbolEqualityComparer.Default.Equals(implementation, symbol))
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        private static bool IsWordBoundary(string name, int index)
        {
            char previous = name[index - 1];
            char current = name[index];
            if (char.IsDigit(current) != char.IsDigit(previous))
            {
                return true;
            }

            if (!char.IsUpper(current))
            {
                return false;
            }

            if (char.IsLower(previous))
            {
                return true;
            }

            bool isNextLower = index + 1 < name.Length && char.IsLower(name[index + 1]);
            return char.IsUpper(previous) && isNextLower;
        }

        private static void FlushWord(List<string> words, StringBuilder current)
        {
            if (current.Length == 0)
            {
                return;
            }

            words.Add(current.ToString());
            current.Clear();
        }

        private static bool IsLettersAndDigits(string name, int startIndex)
        {
            for (int index = startIndex; index < name.Length; index++)
            {
                if (!char.IsLetterOrDigit(name[index]))
                {
                    return false;
                }
            }

            return true;
        }

        private static bool IsAllUpperLetters(string word)
        {
            foreach (char character in word)
            {
                if (!char.IsLetter(character) || !char.IsUpper(character))
                {
                    return false;
                }
            }

            return true;
        }
    }
}
