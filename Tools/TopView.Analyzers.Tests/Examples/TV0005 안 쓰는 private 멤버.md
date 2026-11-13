분석기: UnusedPrivateMemberAnalyzer

# TV0005 안 쓰는 private 멤버

타입 안 어디에서도 참조하지 않는 private 필드, 상수, 메서드, 프로퍼티를 지운다.

통과하는 것:
- 속성이 붙은 멤버(`[SerializeField]`, `[ContextMenu]` 등). Unity가 값을 넣거나 호출한다.
- Unity 메시지 메서드(`Awake`, `OnEnable` 등). Unity가 이름으로 찾아 호출한다.

## 참조 없는 멤버만 경고

```csharp
using UnityEngine;

namespace TopView.Core
{
    public class Sample : MonoBehaviour
    {
        [SerializeField] private int _serialized;
        private int _used;
        private int _unused; // 경고: TV0005
        private const int UNUSED_CONSTANT = 1; // 경고: TV0005
        private int UnusedProperty => 0; // 경고: TV0005

        public int Read()
        {
            return Helper() + _used;
        }

        private int Helper() { return 0; }
        private void NeverCalled() { } // 경고: TV0005
        private void Awake() { }
        [ContextMenu("Run")] private void FromMenu() { }
    }
}
```
