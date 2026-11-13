분석기: LegacyInputAnalyzer

# TV0025 구 Input API 금지

구 입력 API `UnityEngine.Input`을 쓰지 않고 새 Input System으로 읽는다.
이름만 같은 프로젝트의 `Input` 클래스는 통과한다.

## UnityEngine.Input은 경고

```csharp
using UnityEngine;

namespace TopView.Core
{
    public class Sample
    {
        public bool Read()
        {
            return Input.GetKey(KeyCode.A); // 경고: TV0025
        }
    }
}
```

## 이름만 같은 프로젝트 클래스는 통과

```csharp
namespace TopView.Core
{
    public static class Input
    {
        public static bool GetKey() { return false; }
    }

    public class Sample
    {
        public bool Read()
        {
            return Input.GetKey();
        }
    }
}
```
