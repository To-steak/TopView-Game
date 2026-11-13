using System;
using Microsoft.CodeAnalysis;

namespace TopView.Analyzers
{
    /// <summary>
    /// 분석기가 검사할 어셈블리를 고른다.
    /// 분석기 DLL이 모든 어셈블리에 적용되므로, 외부 패키지 코드는 여기서 걸러 낸다.
    /// </summary>
    public static class AnalyzerScopeUtility
    {
        private static readonly string[] DATA_SUFFIXES = { "Component", "Entity" };

        private const string ASSEMBLY_PREFIX = "TopView";
        private const string SCRIPT_ROOT = "Assets/Scripts/";

        public static string RootNamespace => ASSEMBLY_PREFIX;

        /// <summary>
        /// 파일 경로에서 스크립트 루트 아래의 상대 경로를 꺼낸다. 구분자는 '/'로 맞춘다.
        /// </summary>
        /// <param name="filePath">컴파일러가 받은 파일 경로. 상대 경로와 절대 경로 모두 받는다</param>
        /// <returns>스크립트 루트 아래의 경로(예: <c>Core/World.cs</c>). 스크립트 루트 밖이면 null</returns>
        public static string GetScriptRelativePath(string filePath)
        {
            if (string.IsNullOrEmpty(filePath))
            {
                return null;
            }

            string normalizedPath = filePath.Replace('\\', '/');
            int rootIndex = normalizedPath.IndexOf(SCRIPT_ROOT, StringComparison.OrdinalIgnoreCase);
            if (rootIndex < 0)
            {
                return null;
            }

            return normalizedPath.Substring(rootIndex + SCRIPT_ROOT.Length);
        }

        /// <summary>
        /// 프로젝트 어셈블리에 선언된 struct인지 확인한다. enum과 Unity·.NET의 struct(Vector3 등)는 아니다.
        /// </summary>
        /// <param name="type">검사할 타입</param>
        /// <returns>프로젝트 struct면 true</returns>
        public static bool IsProjectStruct(ITypeSymbol type)
        {
            if (type == null || type.TypeKind != TypeKind.Struct || type.ContainingAssembly == null)
            {
                return false;
            }

            return type.ContainingAssembly.Name.StartsWith(ASSEMBLY_PREFIX, StringComparison.Ordinal);
        }

        /// <summary>
        /// 타입 이름이 컴포넌트·엔티티 접미사(<c>~Component</c>, <c>~Entity</c>)로 끝나는지 확인한다.
        /// </summary>
        /// <param name="name">검사할 타입 이름</param>
        /// <returns>접미사로 끝나면 true</returns>
        public static bool HasDataSuffix(string name)
        {
            foreach (string suffix in DATA_SUFFIXES)
            {
                if (name.Length > suffix.Length && name.EndsWith(suffix, StringComparison.Ordinal))
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// 프로젝트의 컴포넌트·엔티티 struct인지 확인한다.
        /// </summary>
        /// <param name="type">검사할 타입</param>
        /// <returns>프로젝트 struct이고 이름이 컴포넌트·엔티티 접미사로 끝나면 true</returns>
        public static bool IsDataStruct(ITypeSymbol type)
        {
            return IsProjectStruct(type) && HasDataSuffix(type.Name);
        }

        /// <summary>
        /// 컴파일 중인 어셈블리가 프로젝트 코드인지 확인한다.
        /// </summary>
        /// <param name="compilation">검사할 컴파일</param>
        /// <returns>어셈블리 이름이 프로젝트 접두사로 시작하면 true</returns>
        public static bool IsProjectAssembly(Compilation compilation)
        {
            string assemblyName = compilation.AssemblyName;
            if (assemblyName == null)
            {
                return false;
            }

            return assemblyName.StartsWith(ASSEMBLY_PREFIX, StringComparison.Ordinal);
        }
    }
}
