분석기: UnnecessaryUsingAnalyzer

# TV0004 불필요한 using

파일 안에서 쓰지 않는 `using`을 지운다.
타입을 쓰거나 확장 메서드(`values.Count()`)를 쓰면 쓰는 것으로 본다. 별칭 using(`using A = …;`)과 `using static`은 검사하지 않는다.

## 쓰지 않는 using만 경고

`System.IO`만 아무 데서도 쓰지 않는다. `System.Linq`는 확장 메서드 `Count()` 때문에 쓰는 것이다.

```csharp
using System.Collections.Generic;
using System.IO; // 경고: TV0004
using System.Linq;
using System.Text;
using Alias = System.Console;
using static System.Math;

namespace TopView.Core
{
    public class Sample
    {
        public int Run(List<int> values)
        {
            StringBuilder builder = new StringBuilder();
            return values.Count();
        }
    }
}
```

## 알려진 한계: 문서 주석의 cref에서만 쓰는 using도 경고

Unity는 문서 주석을 해석하지 않고 컴파일해서, `<see cref="…"/>`에서만 쓰는 using은 쓰지 않는 것으로 보인다.

```csharp
using System.Text; // 경고: TV0004

namespace TopView.Core
{
    /// <summary>참조 <see cref="StringBuilder"/></summary>
    public class Sample
    {
    }
}
```
