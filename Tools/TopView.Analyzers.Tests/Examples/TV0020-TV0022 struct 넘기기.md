분석기: StructPassingAnalyzer

# TV0020~TV0022 struct를 복사 없이 넘기기

| ID | 규칙 |
|---|---|
| TV0020 | 컴포넌트·엔티티 배열의 원소를 값으로 꺼내지 않는다. `ref`로, 읽기만 하면 `ref readonly`로 꺼낸다 |
| TV0021 | 프로젝트 struct 매개변수를 값으로 받지 않는다. 고치면 `ref`, 읽기만 하면 `in` |
| TV0022 | 고치지 않는 `ref` 매개변수는 `in`으로 받는다 |

통과하는 것:
- 컴포넌트·엔티티가 아닌 struct 배열(예: 요청 템플릿)의 복사. 일부러 복사해서 고치는 경우가 있다.
- 기본 타입(`int`)과 Unity·.NET의 struct.
- 재정의(`override`)처럼 외부가 시그니처를 정한 메서드.
- `ref` 매개변수의 필드를 다시 `ref`로 넘기는 것(`Increment(ref target.value)`). 고치는 것으로 본다.

## 복사, 값 매개변수, 고치지 않는 ref

`Run` 줄의 경고 둘은 `byValue`(값으로 받음, TV0021)와 `unchanged`(고치지 않는 ref, TV0022)다.

```csharp
namespace TopView.Core
{
    public struct SampleComponent
    {
        public int value;
    }

    public struct SampleRequest
    {
        public int value;
    }

    public class Sample
    {
        public void Run(SampleComponent byValue, ref SampleComponent unchanged, ref SampleComponent changed, in SampleComponent readOnly, int primitive) // 경고: TV0021, TV0022
        {
            SampleComponent[] components = new SampleComponent[1];
            SampleComponent copy = components[0]; // 경고: TV0020
            ref SampleComponent element = ref components[0];
            ref readonly SampleComponent readOnlyElement = ref components[0];
            foreach (SampleComponent item in components) { } // 경고: TV0020
            SampleRequest[] templates = new SampleRequest[1];
            SampleRequest request = templates[0];
            changed.value = 1;
            int read = unchanged.value + readOnly.value + byValue.value + copy.value + element.value + readOnlyElement.value + request.value + primitive;
        }

        public void Take(SampleRequest request) // 경고: TV0021
        {
        }

        public void Mutate(ref SampleComponent target)
        {
            Increment(ref target.value);
        }

        public override bool Equals(object other) { return false; }

        public override int GetHashCode() { return 0; }

        private static void Increment(ref int number)
        {
            number++;
        }
    }
}
```
