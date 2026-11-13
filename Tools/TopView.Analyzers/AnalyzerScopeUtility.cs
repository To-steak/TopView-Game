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
        private const string ASSEMBLY_PREFIX = "TopView";

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
