분석기: VarDeclarationAnalyzer

# TV0002 var 금지

`var`로 선언하지 않고 타입을 적는다. 지역 변수, foreach, `out var`, `is var`, 분해 선언을 모두 잡는다.

## var의 모든 모양

```csharp
namespace TopView.Core
{
    public class Sample
    {
        public void Run(object value, System.Collections.Generic.List<int> list)
        {
            var count = 1; // 경고: TV0002
            foreach (var item in list) { } // 경고: TV0002
            int.TryParse("1", out var parsed); // 경고: TV0002
            if (value is var any) { } // 경고: TV0002
            var (first, second) = (1, 2); // 경고: TV0002
            int explicitCount = count;
        }
    }
}
```
