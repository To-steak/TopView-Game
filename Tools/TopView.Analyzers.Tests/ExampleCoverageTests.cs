using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Xunit;

namespace TopView.Analyzers.Tests
{
    /// <summary>
    /// 예시 문서가 모든 분석기와 모든 규칙을 덮는지 검사한다. 새 규칙을 만들고 예시를 빠뜨리면 실패한다.
    /// </summary>
    public sealed class ExampleCoverageTests
    {
        private static readonly Regex EXPECT_PATTERN = new Regex(@"//\s*경고:\s*(?<identifiers>TV[0-9]{4}(\s*,\s*TV[0-9]{4})*)\s*$");

        [Fact]
        public void EveryAnalyzerHasAnExampleDocument()
        {
            HashSet<string> documentedAnalyzers = new HashSet<string>();
            foreach (string fileName in ExampleDocument.GetDocumentFileNames())
            {
                foreach (ExampleSection section in ExampleDocument.Load(fileName))
                {
                    documentedAnalyzers.Add(section.AnalyzerName);
                }
            }

            List<string> missingAnalyzers = new List<string>();
            foreach (Type analyzerType in GetAnalyzerTypes())
            {
                if (!documentedAnalyzers.Contains(analyzerType.Name))
                {
                    missingAnalyzers.Add(analyzerType.Name);
                }
            }

            Assert.True(missingAnalyzers.Count == 0, "예시 문서가 없는 분석기: " + string.Join(", ", missingAnalyzers));
        }

        [Fact]
        public void EveryRuleHasAWarningExample()
        {
            HashSet<string> exampleIdentifiers = new HashSet<string>();
            foreach (string fileName in ExampleDocument.GetDocumentFileNames())
            {
                foreach (ExampleSection section in ExampleDocument.Load(fileName))
                {
                    foreach (TestSource source in section.Sources)
                    {
                        CollectIdentifiers(source.Text, exampleIdentifiers);
                    }
                }
            }

            List<string> missingIdentifiers = new List<string>();
            foreach (Type analyzerType in GetAnalyzerTypes())
            {
                DiagnosticAnalyzer analyzer = (DiagnosticAnalyzer)Activator.CreateInstance(analyzerType);
                foreach (DiagnosticDescriptor descriptor in analyzer.SupportedDiagnostics)
                {
                    if (!exampleIdentifiers.Contains(descriptor.Id))
                    {
                        missingIdentifiers.Add(descriptor.Id);
                    }
                }
            }

            Assert.True(missingIdentifiers.Count == 0, "경고 예시가 없는 규칙: " + string.Join(", ", missingIdentifiers));
        }

        private static void CollectIdentifiers(string text, HashSet<string> identifiers)
        {
            foreach (string line in text.Split('\n'))
            {
                Match match = EXPECT_PATTERN.Match(line.TrimEnd());
                if (!match.Success)
                {
                    continue;
                }

                foreach (string identifier in match.Groups["identifiers"].Value.Split(','))
                {
                    identifiers.Add(identifier.Trim());
                }
            }
        }

        private static List<Type> GetAnalyzerTypes()
        {
            List<Type> analyzerTypes = new List<Type>();
            foreach (Type type in typeof(AnalyzerScopeUtility).Assembly.GetTypes())
            {
                if (!type.IsAbstract && typeof(DiagnosticAnalyzer).IsAssignableFrom(type))
                {
                    analyzerTypes.Add(type);
                }
            }

            return analyzerTypes;
        }
    }
}
