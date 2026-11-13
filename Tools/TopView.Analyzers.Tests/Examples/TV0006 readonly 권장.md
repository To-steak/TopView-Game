분석기: ReadonlyFieldAnalyzer

# TV0006 readonly 권장

선언할 때나 생성자에서만 값을 넣고, 그 뒤로 바꾸지 않는 private 필드에는 `readonly`를 붙인다.

통과하는 것:
- 속성이 붙은 필드(`[SerializeField]` 등). Unity가 값을 넣는다.
- 생성자 밖(예: `Awake`)에서 대입하거나 `++`로 바꾸는 필드.
- struct 필드의 멤버에 쓰는 필드(`_position.x = 1f`). 읽기만 하면 경고한다.
- 이미 `readonly`인 필드.

## 생성자 밖에서 바뀌지 않는 필드만 경고

```csharp
using System.Collections.Generic;
using UnityEngine;

namespace TopView.Core
{
    public class Sample : MonoBehaviour
    {
        [SerializeField] private int _serialized;
        private int _neverAssigned; // 경고: TV0006
        private int _onlyInConstructor; // 경고: TV0006
        private int _setInAwake;
        private int _incremented;
        private Vector3 _position;
        private Vector3 _readPosition; // 경고: TV0006
        private List<int> _list = new List<int>(); // 경고: TV0006
        private static int _staticCounter; // 경고: TV0006
        private readonly int _alreadyReadonly;

        public Sample()
        {
            _onlyInConstructor = 1;
        }

        public float Read()
        {
            _incremented++;
            _position.x = 1f;
            _list.Add(_onlyInConstructor);
            return _setInAwake + _incremented + _list.Count + _staticCounter + _neverAssigned + _readPosition.x + _serialized + _alreadyReadonly + _position.y;
        }

        private void Awake()
        {
            _setInAwake = 2;
        }
    }
}
```

`_list`는 내용(`Add`)은 바뀌지만 다른 목록으로 바꿔 끼우지 않으므로 `readonly`를 붙일 수 있다.
